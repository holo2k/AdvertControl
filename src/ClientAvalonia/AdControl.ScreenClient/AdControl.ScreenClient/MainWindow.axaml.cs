using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdControl.ScreenClient.Enums;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdControl.ScreenClient;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly CancellationTokenSource _cts = new();
    private readonly int _intervalSeconds;
    private readonly PollingService _polling;
    private long _knownVersion;
    private string _screenId;
    private List<ExpandoObject>? _tableView;

    public MainWindow()
    {
        InitializeComponent();
        VideoPlayerControl.Player?.InitializeAsync();
        // DI
        _polling = App.Services?.GetRequiredService<PollingService>()
                   ?? throw new InvalidOperationException("DI service not initialized");

        var cfg = App.Services?.GetService<IConfiguration>();
        _screenId = cfg?["Screen:Id"] ?? Environment.GetEnvironmentVariable("SCREEN_ID") ?? string.Empty;
        _intervalSeconds = int.TryParse(cfg?["Polling:IntervalSeconds"], out var s) ? s : 5;


        // Bind DataContext for ListBox (if Items is bound to ListBox in XAML)
        DataContext = this;

        if (string.IsNullOrWhiteSpace(_screenId))
            StatusText.Text = "ScreenId not set. Use pairing or set SCREEN_ID.";
        else
            StatusText.Text = $"ScreenId={_screenId}";

        Items = new ObservableCollection<ConfigItemDto>
        {
            //просто заглушки для файлов 
            new("1", "Video", "file:///C:/123.mp4", "inlineData1", "checksum1", 1024, 5, 1),
            new("2", "Image", "C:/321.png", "inlineData2", "checksum2", 2048, 5, 2),
            new("3", "Table", "C:/data.json", "inlineData3", "checksum3", 512, 5, 3)
        };

        // Start polling loop
        StartLoopAsync(_cts.Token);

        PairCodeText.Content = "CODE";

        SetState(ScreenState.NotPaired);

        // Start ShowItemsAsync immediately to continuously show items
        ShowItemsAsync(_cts.Token);

        DataContext = this;
    }

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

    // ?????????? ??? ???????? ????????
    public ConfigItemDto? CurrentItem { get; set; }

    public ScreenState State { get; private set; } = ScreenState.NotPaired;

    public ObservableCollection<ConfigItemDto> Items { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;


    private async Task<List<ExpandoObject>?> GetDynamicListFromJson(string jsonPath)
    {
        var json = await File.ReadAllTextAsync(jsonPath);

        //НЕ УДАЛЯТЬ
        // using var stream = await httpClient.GetStreamAsync(jsonPath);
        // using var reader = new StreamReader(stream);
        // var json = await reader.ReadToEndAsync();

        var rows = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json)!;

        if (rows == null || rows.Count == 0) return null;

        var dynamicList = new List<ExpandoObject>();

        foreach (var rowDict in rows)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var pair in rowDict)
            {
                var key = pair.Key;
                var element = pair.Value;
                object value;

                // Надежное извлечение чистого значения из JsonElement
                value = element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.TryGetDecimal(out var d) ? d : element.GetRawText(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => element.GetRawText()
                };

                // Добавляем пару ключ-значение в ExpandoObject
                expando.Add(key, value!);
            }

            dynamicList.Add((ExpandoObject)expando);
        }

        return dynamicList;
    }

    private void SetState(ScreenState state)
    {
        State = state;
        StartPairButton.IsVisible = state == ScreenState.NotPaired;
        PairCodeText.IsVisible = state == ScreenState.Pairing;
        ItemsList.IsVisible = state == ScreenState.Paired;
    }

    private async Task StartLoopAsync(CancellationToken token)
    {
        try
        {
            // Implement polling loop logic here
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            // Handle error
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Loop error: {ex.Message}");
        }
    }

    private async Task PollOnce(CancellationToken token)
    {
        try
        {
            StatusText.Text = $"Polling... knownVersion={_knownVersion}";

            // Sample data for testing (replace with real polling logic)
            var cfg = new ConfigDto(
                1,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                new[]
                {
                    new ConfigItemDto("1", "Video", "file:///C:/123.mp4", "inlineData1", "checksum1", 1024, 5, 1),
                    new ConfigItemDto("2", "Image", "C:/321.png", "inlineData2", "checksum2", 2048, 5, 2),
                    new ConfigItemDto("3", "Table", "C:/data.json", "inlineData3", "checksum3", 512, 5,
                        3)
                }
            );

            if (cfg == null)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                    StatusText.Text = "No response (gateway and direct grpc unavailable)");
                return;
            }

            if (cfg.NotModified)
            {
                await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Not modified (v={_knownVersion})");
                return;
            }

            _knownVersion = cfg.Version;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Items.Clear();
                foreach (var i in cfg.Items ?? Array.Empty<ConfigItemDto>())
                    Items.Add(new ConfigItemDto(i.Id, i.Type, i.Url, i.InlineData, i.Checksum, i.Size,
                        i.DurationSeconds, i.Order));

                StatusText.Text = $"Loaded v={cfg.Version}, items={cfg.Items?.Length ?? 0}";
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Error: {ex.Message}");
        }
    }

    private async Task ShowItemsAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
                foreach (var item in Items)
                {
                    if (token.IsCancellationRequested) break;

                    CurrentItem = item;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        VideoPlayerControl.IsVisible = false;
                        ImageControl.IsVisible = false;
                        JsonTable.IsVisible = false;
                    });

                    if (item.Type == "Video")
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            VideoPlayerControl.IsVisible = true;
                            await Task.Yield();
                            VideoPlayerControl.Player.Source = new UriSource(item.Url);
                            VideoPlayerControl.Player.IsLoopingEnabled = true;
                            VideoPlayerControl.Player.PrepareAsync();
                            VideoPlayerControl.Player.PlayAsync();
                            await Task.Delay(TimeSpan.FromSeconds(item.DurationSeconds), token);
                            VideoPlayerControl.Player.PauseAsync();
                        });
                    }
                    else if (item.Type == "Image")
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ImageControl.IsVisible = true;
                            ImageControl.Source = new Bitmap(item.Url);
                        });

                        await Task.Delay(TimeSpan.FromSeconds(item.DurationSeconds), token);
                    }
                    else if (item.Type == "Table")
                    {
                        var dynamicList = await GetDynamicListFromJson(item.Url);

                        if (dynamicList is null || dynamicList.Count == 0) return;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            JsonTable.Columns.Clear();

                            var firstRow = dynamicList.First() as IDictionary<string, object>;

                            foreach (var colName in firstRow!.Keys)
                            {
                                var gridColumn = new DataGridTextColumn
                                {
                                    Header = colName,
                                    Binding = new Binding(".")
                                    {
                                        Converter = new ExpandoPropertyConverter(),
                                        ConverterParameter = colName,
                                        Mode = BindingMode.OneWay
                                    }
                                };
                                JsonTable.Columns.Add(gridColumn);
                            }

                            JsonTable.IsVisible = true;
                            JsonTable.ItemsSource = dynamicList;
                        });
                        await Task.Delay(TimeSpan.FromSeconds(item.DurationSeconds), token);
                    }
                }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts.Cancel();
        base.OnClosed(e);
    }

    private async void StartPairButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts.TryReset();
        }

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

        var started = false;
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

                    // Start polling loop after pairing
                    _ = StartLoopAsync(_cts.Token);

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
}