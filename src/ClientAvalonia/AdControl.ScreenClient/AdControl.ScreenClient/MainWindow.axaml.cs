using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using AdControl.ScreenClient.Enums;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdControl.ScreenClient;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly int _intervalSeconds;
    private readonly PlayerService _player;
    private readonly PollingService _polling;
    private CancellationTokenSource _cts = new();
    private long _knownVersion;
    private string _screenId;
    private List<ExpandoObject>? _tableView;

    public MainWindow()
    {
        InitializeComponent();

        _player = new PlayerService(VideoViewControl, ImageControl, JsonTable);
        _polling = App.Services?.GetRequiredService<PollingService>()
                   ?? throw new InvalidOperationException("PollingService not found");

        var cfg = App.Services?.GetService<IConfiguration>();
        _screenId = cfg?["Screen:Id"] ?? Environment.GetEnvironmentVariable("SCREEN_ID") ?? string.Empty;

        _intervalSeconds = int.TryParse(cfg?["Polling:IntervalSeconds"], out var s) ? s : 5;

        DataContext = this;

        StatusText.Text = string.IsNullOrWhiteSpace(_screenId)
            ? "ScreenId not set. Use pairing or set SCREEN_ID."
            : $"ScreenId={_screenId}";

        SetState(ScreenState.NotPaired);

        _ = StartAsync(_cts.Token);
    }

    public ObservableCollection<ConfigItemDto> Items { get; } = new();

    public ConfigItemDto? CurrentItem { get; set; }

    public ScreenState State { get; private set; } = ScreenState.NotPaired;

    public List<ExpandoObject>? TableView
    {
        get => _tableView;
        set
        {
            if (_tableView != value)
            {
                _tableView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableView)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task StartAsync(CancellationToken token)
    {
        var showTask = Task.Run(() => ShowItemsAsync(token), token);
        var loopTask = Task.Run(() => StartLoopAsync(token), token);

        await Task.WhenAll(showTask, loopTask);
    }


    private void SetState(ScreenState state)
    {
        State = state;
        StartPairButton.IsVisible = state == ScreenState.NotPaired;
        PairCodeText.IsVisible = state == ScreenState.Pairing;
        ItemsList.IsVisible = state == ScreenState.Paired;
    }

    private async Task<List<ExpandoObject>?> GetDynamicListFromJson(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return null;

        var json = await File.ReadAllTextAsync(jsonPath);

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

    private async Task StartLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await PollOnce(token);
                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                StatusText.Text = $"Loop error: {ex.Message}");
        }
    }


    private async Task PollOnce(CancellationToken token)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusText.Text = $"Polling... knownVersion={_knownVersion}";
            });

            var cfg = await _polling.GetConfigAsync(_screenId, _knownVersion);

            if (cfg == null) throw new Exception("Config is empty");

            _knownVersion = cfg.Version;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Items.Clear();
                foreach (var i in cfg.Items ?? Array.Empty<ConfigItemDto>())
                    Items.Add(i);

                StatusText.Text = $"Loaded v={cfg.Version}, items={cfg.Items?.Length ?? 0}";
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = $"Error: {ex.Message}"; });
        }
    }


    private async Task ShowItemsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var currentVersion = _knownVersion;
            var snapshot = await Dispatcher.UIThread.InvokeAsync(() => Items.ToList());

            foreach (var item in snapshot)
            {
                if (_knownVersion != currentVersion)
                    break;

                CurrentItem = item;

                switch (item.Type)
                {
                    case "Video":
                        await _player.ShowVideoAsync(item.Url, item.DurationSeconds, token);
                        break;

                    case "Image":
                        await _player.ShowImageAsync(item.Url, item.DurationSeconds, token);
                        break;

                    case "Table":
                        var rows = await GetDynamicListFromJson(item.Url);
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

    private async void StartPairButton_Click(object? sender, RoutedEventArgs e)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        await StartPairingAsync();
    }

    private void UnpairButton_Click(object? sender, RoutedEventArgs e)
    {
        _cts.Cancel();
        _knownVersion = 0;
        _screenId = string.Empty;
        Items.Clear();
        StatusText.Text = "Screen unpaired.";
        SetState(ScreenState.NotPaired);
    }

    public async Task StartPairingAsync(int ttlMinutes = 10, string? info = null)
    {
        SetState(ScreenState.Pairing);

        var tempId = Guid.NewGuid().ToString("N");
        var code = new Random().Next(0, 1000000).ToString("D6");

        PairCodeText.Content = code;

        bool started;
        try
        {
            started = await _polling.StartPairAsync(tempId, code, ttlMinutes, info);
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Pair start failed: {ex.Message}";
            SetState(ScreenState.NotPaired);
            return;
        }

        if (!started)
        {
            StatusText.Text = "Pair start rejected by server.";
            SetState(ScreenState.NotPaired);
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
                    StatusText.Text = $"Paired. ScreenId={_screenId}";
                    SetState(ScreenState.Paired);

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

        StatusText.Text = "Pairing timed out.";
        SetState(ScreenState.NotPaired);
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
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Saving config failed: {ex.Message}");
        }
    }
}