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
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibVLCSharp.Shared;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace AdControl.ScreenClient;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private CancellationTokenSource _cts = new();
    private readonly int _intervalSeconds;
    private readonly PollingService _polling;
    private long _knownVersion;
    private string _screenId;
    private List<ExpandoObject>? _tableView;
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;

    public MainWindow()
    {
        InitializeComponent();

        Core.Initialize();
        _libVLC = new LibVLC(enableDebugLogs: true);
        _mediaPlayer = new MediaPlayer(_libVLC);

        VideoViewControl.AttachedToVisualTree += (s, e) =>
        {
            VideoViewControl.MediaPlayer = _mediaPlayer;
        };

        _polling = App.Services?.GetRequiredService<PollingService>()
                   ?? throw new InvalidOperationException("DI service not initialized");

        var cfg = App.Services?.GetService<IConfiguration>();
        _screenId = cfg?["Screen:Id"] ?? Environment.GetEnvironmentVariable("SCREEN_ID") ?? string.Empty;
        _intervalSeconds = int.TryParse(cfg?["Polling:IntervalSeconds"], out var s) ? s : 5;

        DataContext = this;

        StatusText.Text = string.IsNullOrWhiteSpace(_screenId)
            ? "ScreenId not set. Use pairing or set SCREEN_ID."
            : $"ScreenId={_screenId}";

        Items = new ObservableCollection<ConfigItemDto>
        {
            new("1", "Table", "B:/example.json", "inlineData3", "checksum3", 512, 5, 1),
            new("2", "Image", "B:/example.jpg", "inlineData2", "checksum2", 2048, 5, 2),
            new("3", "Video", "B:/example.mp4", "inlineData1", "checksum1", 1024, 5, 3),
        };

        PairCodeText.Content = "CODE";
        SetState(ScreenState.NotPaired);

        //_ = StartLoopAsync(_cts.Token);
        _ = ShowItemsAsync(_cts.Token);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
                /*await PollOnce(token);
                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);*/
            }
        }
        catch (TaskCanceledException) { }
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
            StatusText.Text = $"Polling... knownVersion={_knownVersion}";

            var cfg = new ConfigDto(
                1,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                new[]
                {
                    new ConfigItemDto("1", "Image", "C:/321.png", "", "", 2048, 5, 2),
                    new ConfigItemDto("2", "Video", "file:///C:/123.mp4", "", "", 1024, 5, 1),
                    new ConfigItemDto("3", "Table", "C:/data.json", "", "", 512, 5, 3)
                });

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
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Error: {ex.Message}");
        }
    }

    private async Task ShowItemsAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var item in Items)
                {
                    if (token.IsCancellationRequested)
                        break;

                    CurrentItem = item;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        VideoViewControl.IsVisible = false;
                        ImageControl.IsVisible = false;
                        JsonTable.IsVisible = false;
                    });

                    switch (item.Type)
                    {
                        case "Video":
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                VideoViewControl.IsVisible = true;
                                ImageControl.IsVisible = false;
                                JsonTable.IsVisible = false;

                                if (VideoViewControl.MediaPlayer == null)
                                    VideoViewControl.MediaPlayer = _mediaPlayer;
                            });

                            _mediaPlayer.Stop();

                            Media? media = null;
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                               
                                media = new Media(_libVLC, item.Url);
                                _mediaPlayer.Play(media);
                            });

                            await Task.Delay(TimeSpan.FromSeconds(item.DurationSeconds), token);

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _mediaPlayer.Stop();
                                media?.Dispose();
                            });
                            break;


                        case "Image":
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                ImageControl.IsVisible = true;
                                ImageControl.Source = new Bitmap(item.Url);
                            });
                            await Task.Delay(TimeSpan.FromSeconds(item.DurationSeconds), token);
                            break;

                        case "Table":
                            var dynamicList = await GetDynamicListFromJson(item.Url);
                            if (dynamicList is null || dynamicList.Count == 0)
                                break;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                JsonTable.Columns.Clear();
                                var firstRow = dynamicList.First() as IDictionary<string, object>;
                                foreach (var col in firstRow!.Keys)
                                {
                                    JsonTable.Columns.Add(new DataGridTextColumn
                                    {
                                        Header = col,
                                        Binding = new Binding(".")
                                        {
                                            Converter = new ExpandoPropertyConverter(),
                                            ConverterParameter = col,
                                            Mode = BindingMode.OneWay
                                        }
                                    });
                                }

                                JsonTable.IsVisible = true;
                                JsonTable.ItemsSource = dynamicList;
                            });
                            await Task.Delay(TimeSpan.FromSeconds(item.DurationSeconds), token);
                            break;
                    }
                }
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts.Cancel();
        _mediaPlayer.Stop();
        _mediaPlayer.Dispose();
        _libVLC.Dispose();
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
                    return;
                }
            }
            catch { }

            await Task.Delay(2000, _cts.Token);
        }

        StatusText.Text = "Pairing timed out.";
        SetState(ScreenState.NotPaired);
    }
}
