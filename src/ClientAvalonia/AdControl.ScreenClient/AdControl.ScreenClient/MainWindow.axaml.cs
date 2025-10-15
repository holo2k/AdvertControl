using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AdControl.ScreenClient.Enums;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdControl.ScreenClient;

public partial class MainWindow : Window
{
    private readonly CancellationTokenSource _cts = new();
    private readonly int _intervalSeconds;
    private readonly PollingService _polling;
    private long _knownVersion;
    private string _screenId;

    public MainWindow()
    {
        InitializeComponent();

        // DI
        _polling = App.Services?.GetRequiredService<PollingService>()
                   ?? throw new InvalidOperationException("DI контейнер не инициализирован");

        var cfg = App.Services?.GetService<IConfiguration>();
        _screenId = cfg?["Screen:Id"] ?? Environment.GetEnvironmentVariable("SCREEN_ID") ?? string.Empty;
        _intervalSeconds = int.TryParse(cfg?["Polling:IntervalSeconds"], out var s) ? s : 5;

        // привязка коллекции к ListBox (если в XAML Items="{Binding Items}")
        DataContext = this;

        if (string.IsNullOrWhiteSpace(_screenId))
            StatusText.Text = "ScreenId not set. Use pairing or set SCREEN_ID.";
        else
            StatusText.Text = $"ScreenId={_screenId}";

        // Запускаем цикл опроса в фоне
        _ = StartLoopAsync(_cts.Token);

        PairCodeText.Content = "CODE";

        SetState(ScreenState.NotPaired);
    }

    public ScreenState State { get; private set; } = ScreenState.NotPaired;

    public ObservableCollection<ConfigItemDto> Items { get; } = new();

    private void SetState(ScreenState state)
    {
        State = state;

        StartPairButton.IsVisible = state == ScreenState.NotPaired;
        PairCodeText.IsVisible = state == ScreenState.Pairing;
        ItemsList.IsVisible = state == ScreenState.Paired;
    }

    // Цикл опроса — корректно отменяется при закрытии окна
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
            // обновление UI из UI-потока
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Loop error: {ex.Message}");
        }
    }

    private async Task PollOnce(CancellationToken token)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = $"Polling... knownVersion={_knownVersion}");

            if (string.IsNullOrWhiteSpace(_screenId))
            {
                await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = "No screenId. Start pairing.");
                return;
            }

            var cfg = await _polling.GetConfigAsync(_screenId, _knownVersion);
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
                    // используем реальный DTO, а не анонимный тип
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

    // Начать pairing: генерируем tempDisplayId + 6-значный код, отправляем в gateway, затем опрашиваем статус
    // вызывается из UI (например, кнопка). Возвращаем Task чтобы не использовать async void.
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

                    // начинаем пуллинг конфигурации
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


    protected override void OnClosed(EventArgs e)
    {
        _cts.Cancel();
        base.OnClosed(e);
    }

    private async void StartPairButton_Click(object? sender, RoutedEventArgs e)
    {
        // Если предыдущий токен был отменен, создаем новый CancellationTokenSource
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
}