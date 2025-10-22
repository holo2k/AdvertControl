using System.Dynamic;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace AdControl.ScreenClient.Services;

public class PlayerService : IDisposable
{
    private readonly VideoView _videoView;
    private readonly Image _imageControl;
    private readonly DataGrid _jsonTable;
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;

    public PlayerService(VideoView videoView, Image imageControl, DataGrid jsonTable)
    {
        _videoView = videoView;
        _imageControl = imageControl;
        _jsonTable = jsonTable;

        _libVLC = new LibVLC(enableDebugLogs: true);
        _mediaPlayer = new MediaPlayer(_libVLC);

        _videoView.AttachedToVisualTree += (s, e) =>
        {
            _videoView.MediaPlayer = _mediaPlayer;
        };
    }


    public async Task ShowVideoAsync(string url, int durationSeconds, CancellationToken token)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowOnly(_videoView);

            if (_videoView.MediaPlayer == null)
                _videoView.MediaPlayer = _mediaPlayer;

            _mediaPlayer.Stop();
        });

        using var media = new Media(_libVLC, url);
        _mediaPlayer.Play(media);

        await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);

        await Dispatcher.UIThread.InvokeAsync(() => _mediaPlayer.Stop());
    }


    public async Task ShowImageAsync(string url, int durationSeconds, CancellationToken token)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowOnly(_imageControl);
            _imageControl.Source = new Bitmap(url);
        });

        await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);
    }

    public async Task ShowTableAsync(List<ExpandoObject> rows, int durationSeconds, CancellationToken token)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowOnly(_jsonTable);
            _jsonTable.Columns.Clear();

            if (rows.FirstOrDefault() is IDictionary<string, object> first)
            {
                foreach (var key in first.Keys)
                {
                    _jsonTable.Columns.Add(new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding(".")
                        {
                            Converter = new ExpandoPropertyConverter(),
                            ConverterParameter = key,
                            Mode = BindingMode.OneWay
                        }
                    });
                }
            }

            _jsonTable.ItemsSource = rows;
        });

        await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);
    }

    private void ShowOnly(Control visible)
    {
        _videoView.IsVisible = visible == _videoView;
        _imageControl.IsVisible = visible == _imageControl;
        _jsonTable.IsVisible = visible == _jsonTable;
    }

    public void Dispose()
    {
        try
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _mediaPlayer.Stop();
                _videoView.MediaPlayer = null;
            }).Wait();

            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerService dispose error: {ex}");
        }
    }

}
