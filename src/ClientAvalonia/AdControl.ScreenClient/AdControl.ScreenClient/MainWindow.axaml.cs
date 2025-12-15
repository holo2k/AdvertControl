using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using AdControl.ScreenClient.Enums;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QRCoder;

namespace AdControl.ScreenClient;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly int _intervalSeconds;
    private readonly PlayerService _player;
    private readonly PollingService _polling;
    private CancellationTokenSource _cts = new();
    private List<PlayerWindow>? _playerWindows;
    private const string CreateScreenUrlTemplate = "https://advertcontrol.ru/screens/create-screen?code={0}";

    private long _knownVersion;
    private string _screenId;
    public MainWindow(string? screenId = null, List<ConfigItemDto>? items = null)
    {
        InitializeComponent();

        _player = new PlayerService(VideoViewControl, ImageControl, JsonTable);

        _polling = App.Services?.GetRequiredService<PollingService>()
                   ?? throw new InvalidOperationException("PollingService not found");

        var cfg = App.Services?.GetService<IConfiguration>();

        _playerWindows = new List<PlayerWindow>();
        _screenId = cfg?["Screen:Id"] ?? Environment.GetEnvironmentVariable("SCREEN_ID") ?? string.Empty;
        _intervalSeconds = int.TryParse(cfg?["Polling:IntervalSeconds"], out var s) ? s : 5;

        DataContext = this;
        SetState(ScreenState.NotPaired);

        StatusText.Text = string.IsNullOrWhiteSpace(_screenId)
            ? "ID экрана не установлено. Используйте привязку."
            : $"ID={_screenId}";

        ImageControl.IsVisible = true;

        // Запускаем асинхронную инициализацию
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_screenId))
        {
            await CheckScreenExistAndAdjustAsync(_screenId);

            if (!string.IsNullOrWhiteSpace(_screenId))
            {
                _ = StartAsync(_cts.Token);
            }
            else
            {
                _ = StartPairingLoopAsync(_cts.Token);
            }
        }
        else
        {
            _ = StartPairingLoopAsync(_cts.Token);
        }
    }

    public ObservableCollection<ConfigItemDto> Items { get; } = new();

    public ConfigItemDto? CurrentItem { get; set; }

    public ScreenState State { get; private set; } = ScreenState.NotPaired;

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task StartAsync(CancellationToken token)
    {
        var showTask = Task.Run(() => ShowItemsAsync(token), token);
        var loopTask = Task.Run(() => StartLoopAsync(token), token);
        await Task.WhenAll(showTask, loopTask);
    }
    
    private async Task StartPairingLoopAsync(CancellationToken token)
    {
        SetState(ScreenState.Pairing);

        while (!token.IsCancellationRequested)
        {
            var code = new Random().Next(0, 1000000).ToString("D6");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusText.Inlines = new InlineCollection
                {
                    new Run("Введите код по ссылке\n")
                    {
                        FontSize = 48,
                        Foreground = Brushes.White
                    },
                    new Run("advertcontrol.ru/screens")
                    {
                        FontSize = 56,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brushes.LightSkyBlue,
                    }
                };
            });

            UpdateCodeAndQr(code);

            try
            {
                var tempId = Guid.NewGuid().ToString("N");
                var started = await _polling.StartPairAsync(tempId, code, 5, "auto");
                if (started)
                {
                    var timeout = TimeSpan.FromMinutes(5);
                    var sw = Stopwatch.StartNew();

                    while (sw.Elapsed < timeout && !token.IsCancellationRequested)
                    {
                        var (assigned, assignedScreenId) = await _polling.CheckPairStatusAsync(tempId);
                        if (assigned)
                        {
                            _screenId = assignedScreenId ?? string.Empty;
                            await SaveScreenIdToAppSettingsAsync(_screenId);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                StatusText.Text = $"Экран привязан. ID={_screenId}";
                                SetState(ScreenState.Paired);
                            });
                            _ = StartAsync(token);
                            return;
                        }

                        await Task.Delay(2000, token);
                    }
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                    StatusText.Text = $"Ошибка привязки: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), token); 
        }
    }


    private async Task CheckScreenExistAndAdjustAsync(string screenId)
    {
        var exists = await _polling.IsScreenExistAsync(screenId).ConfigureAwait(false);

        if (exists is false)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                StatusText.Text =
                    "Текущий ID экрана не существует в базе экранов. Он будет удалён и экран будет переключён в состояние подключения.");

            await Task.Delay(TimeSpan.FromSeconds(10));
            await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.NotPaired));
            
            await DeleteScreenId();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.Paired));
        }
    }

    private async Task DeleteScreenId()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            var root = new JsonObject { ["Screen"] = new JsonObject { ["Id"] = string.Empty } };
            await File.WriteAllTextAsync(path,
                root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(false);
        }
    }

    private void SetState(ScreenState state)
    {
        State = state;
        switch (state)
        {
            case ScreenState.Paired:
                PairCodeText.IsVisible = false;
                break;
            case ScreenState.NotPaired:
            case ScreenState.Pairing:
                PairCodeText.IsVisible = true;
                break;
        }
    }

    private async Task<List<ExpandoObject>?> GetDynamicListFromJson(string json)
    {
        //if (!File.Exists(json))
        //    return null;

        //var json = await File.ReadAllTextAsync(jsonPath);

        var rows = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);
        if (rows is null || rows.Count == 0)
            return null;

        var list = new List<ExpandoObject>();
        foreach (var dict in rows)
        {
            var exp = new ExpandoObject() as IDictionary<string, object?>;
            foreach (var pair in dict)
            {
                object? value = pair.Value.ValueKind switch
                {
                    JsonValueKind.String => pair.Value.GetString(),
                    JsonValueKind.Number => pair.Value.TryGetDecimal(out var d) ? d : pair.Value.GetRawText(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => null
                };
                exp[pair.Key] = value;
            }

            list.Add((ExpandoObject)exp);
        }

        return list;
    }

    public async Task StartLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (State != ScreenState.Paired)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                await PollOnce(token);
                if (DateTime.UtcNow.Minute % 5 == 0)
                {
                    await CheckScreenExistAndAdjustAsync(_screenId);
                }
                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                StatusText.Text = $"Ошибка во время цикла обращения к серверу: {ex.Message}");
        }
    }
    
    private void OpenPlayerWindows(ConfigDto cfg)
    {
        var windowCount = cfg.WindowCount > 0 ? cfg.WindowCount : 0;

        if (windowCount <= 1)
            return;

        for (var i = 1; i < windowCount; i++)
        {
            var win = new PlayerWindow(cfg.Items?.ToList(), i+1);
            _playerWindows.Add(win);
            win.Show();
        }
    }
    
    private bool _playerWindowsOpened;

    private async Task PollOnce(CancellationToken token)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = "Обращение к серверу..."; });

            var cfg = await _polling.GetConfigAsync(_screenId, _knownVersion);

            if (cfg == null) throw new Exception("Конфиг пуст.");
            
            
            _knownVersion = cfg.Version;
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Items.Clear();
                foreach (var i in cfg.Items)
                    Items.Add(i);

                StatusText.Text = $"Загружен конфиг с версией {cfg.Version}";
            });

            if (!_playerWindowsOpened)
            {
                _playerWindowsOpened = true;
                await Dispatcher.UIThread.InvokeAsync(() => OpenPlayerWindows(cfg));
            }

            foreach (var pW in _playerWindows)
            {
                await Dispatcher.UIThread.InvokeAsync(() => pW.UpdateItems(cfg.Items.ToList()));
            }

            
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = $"Ошибка: {ex.Message}"; });
        }
    }

    private async Task ShowItemsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (State != ScreenState.Paired) continue;

            var currentVersion = _knownVersion;
            var snapshot = await Dispatcher.UIThread.InvokeAsync(() => Items.ToList());

            foreach (var item in snapshot)
            {
                if (_knownVersion != currentVersion)
                    break;

                CurrentItem = item;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusText.Text = string.Empty;
                    QrImage.IsVisible = false;
                    PairCodeText.IsVisible = false;
                });

                switch (item.Type)
                {
                    case "Video":
                        await _player.ShowVideoAsync(item.Url, item.DurationSeconds, token);
                        break;

                    case "Image":
                        await _player.ShowImageAsync(item.Url, item.DurationSeconds, token);
                        break;

                    case "InlineJson":
                        var rows = await GetDynamicListFromJson(item.InlineData);
                        if (rows != null)
                            await _player.ShowTableAsync(rows, item.DurationSeconds, token);
                        break;
                }

                if (_knownVersion != currentVersion)
                    break;
            }

            await Task.Delay(200, token);
        }
    }


    protected override void OnClosed(EventArgs e)
    {
        _cts.Cancel();

        try
        {
            _player.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerService dispose error: {ex}");
        }

        base.OnClosed(e);
    }
    
    

    public async Task StartPairingAsync(int ttlMinutes = 10, string? info = null)
    {
        await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.Pairing));
        StatusText.Text = "Введите данный код в вашем кабинете - adcontrol.ru/pairing";

        var tempId = Guid.NewGuid().ToString("N");
        var code = new Random().Next(0, 1000000).ToString("D6");

        PairCodeText.Text = code;
        bool started;
        try
        {
            started = await _polling.StartPairAsync(tempId, code, ttlMinutes, info);
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка привязки: {ex.Message}";
            await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.NotPaired));
            return;
        }

        if (!started)
        {
            StatusText.Text = "Привязка отклонена сервером.";
            await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.NotPaired));
            return;
        }

        var timeout = TimeSpan.FromMinutes(ttlMinutes);
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < timeout && !_cts.IsCancellationRequested)
        {
            try
            {
                var (assigned, assignedScreenId) = await _polling.CheckPairStatusAsync(tempId);
                if (assigned)
                {
                    _screenId = assignedScreenId ?? string.Empty;
                    StatusText.Text = $"Связано. ID Экрана = {_screenId}";
                    await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.Paired));

                    _ = StartLoopAsync(_cts.Token);
                    await SaveScreenIdToAppSettingsAsync(_screenId);

                    // Start polling loop after pairing
                    await StartLoopAsync(_cts.Token);

                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(2000, _cts.Token);
        }

        StatusText.Text = "Время привязки вышло.";
        await Dispatcher.UIThread.InvokeAsync(() => SetState(ScreenState.NotPaired));
    }

    private async Task SaveScreenIdToAppSettingsAsync(string screenId)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                // если файла нет, создаём минимальную структуру
                var root = new JsonObject { ["Screen"] = new JsonObject { ["Id"] = screenId } };
                await File.WriteAllTextAsync(path,
                    root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            var node = JsonNode.Parse(json) ?? new JsonObject();

            if (node["Screen"] == null) node["Screen"] = new JsonObject();
            node["Screen"]["Id"] = screenId;

            await File.WriteAllTextAsync(path, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            // Показываем ошибку в UI, но не ломаем поток привязки
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Ошибка сохранения ID: {ex.Message}");
        }
    }

    /// <summary>
    /// Генерирует QR-картинку для given code и устанавливает в QrImage.
    /// Генерация выполняется в background-потоке, UI обновляется через Dispatcher.
    /// </summary>
    private async Task GenerateAndSetQrImageAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            await Dispatcher.UIThread.InvokeAsync(() => QrImage.Source = null);
            return;
        }

        var url = string.Format(CreateScreenUrlTemplate, Uri.EscapeDataString(code));

        try
        {
            // Выполняем тяжелую работу в Task.Run чтобы не блокировать UI
            var pngBytes = await Task.Run(() =>
            {
                using var qrGen = new QRCodeGenerator();
                using var data = qrGen.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                using var qr = new QRCode(data);
                using var bmp = qr.GetGraphic(20); // возвращает System.Drawing.Bitmap

                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }, cancellationToken);

            // Устанавливаем картинку в UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    using var ms = new MemoryStream(pngBytes);
                    var bitmap = new Bitmap(ms);
                    QrImage.Source = bitmap;
                }
                catch
                {
                    QrImage.Source = null;
                }
            });
        }
        catch (OperationCanceledException)
        {
            // игнорируем отмену
        }
        catch (Exception)
        {
            // в случае ошибки — убираем QR
            await Dispatcher.UIThread.InvokeAsync(() => QrImage.Source = null);
        }
    }

    /// <summary>
    /// Обновляет UI: текст кода и генерирует QR.
    /// Вызывать из любого потока.
    /// </summary>
    private void UpdateCodeAndQr(string code)
    {
        // обновляем текст в UI-потоке
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            PairCodeText.Text = code;
        });

        // запускаем генерацию QR (не ждём)
        _ = GenerateAndSetQrImageAsync(code, _cts.Token);
    }
}