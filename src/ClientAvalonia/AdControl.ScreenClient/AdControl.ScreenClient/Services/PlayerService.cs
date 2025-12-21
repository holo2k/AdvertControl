using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace AdControl.ScreenClient.Services;

public class PlayerService : IDisposable
{
    private const string GatewayBaseUrl = "https://advertcontrol.ru/files/";
    private readonly Image _imageControl;
    private readonly DataGrid _jsonTable;
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private readonly VideoView _videoView;
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _cacheDir;

    public PlayerService(VideoView videoView, Image imageControl, DataGrid jsonTable, IHttpClientFactory httpFactory)
    {
        _videoView = videoView;
        _imageControl = imageControl;
        _jsonTable = jsonTable;
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));

        _libVLC = new LibVLC(true);
        _mediaPlayer = new MediaPlayer(_libVLC);

        _videoView.AttachedToVisualTree += (s, e) => { _videoView.MediaPlayer = _mediaPlayer; };

        _cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AdControl", "cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public void Dispose()
    {
        try
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                { _mediaPlayer.Pause(); }
                catch { }
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

    // New API: accept ConfigItemDto (recommended)
    public Task ShowVideoAsync(ConfigItemDto item, CancellationToken token) =>
        ShowVideoAsync(item.Url, item.Checksum, item.DurationSeconds, token);

    public Task ShowImageAsync(ConfigItemDto item, CancellationToken token, bool isMainWindow = true) =>
        ShowImageAsync(item.Url, item.Checksum, item.DurationSeconds, token, isMainWindow);

    // Backwards-compatible: existing signature but uses checksum lookup by Id if checksum empty (not ideal)
    public Task ShowVideoAsync(string fileName, int durationSeconds, CancellationToken token) =>
        ShowVideoAsync(fileName, checksum: null, durationSeconds, token);

    public async Task ShowVideoAsync(string fileName, string? checksum, int durationSeconds, CancellationToken token)
    {
        var localPath = await GetCachedFilePathAsync(fileName, checksum, token);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowOnly(_videoView);

            if (_videoView.MediaPlayer != _mediaPlayer)
                _videoView.MediaPlayer = _mediaPlayer;

            _mediaPlayer.Stop();
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Use local path via FromPath
            using var media = new Media(_libVLC, localPath, FromType.FromPath);
            _mediaPlayer.Play(media);
        });

        await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);

        await Dispatcher.UIThread.InvokeAsync(() => { _mediaPlayer.Stop(); });
    }

    public async Task ShowImageAsync(string fileName, string? checksum, int durationSeconds, CancellationToken token, bool isMainWindow = true)
    {
        var localPath = await GetCachedFilePathAsync(fileName, checksum, token);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowOnly(_imageControl);
        });

        // load bitmap from file stream
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var fs = File.OpenRead(localPath);
            _imageControl.Source = new Bitmap(fs);
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
                foreach (var key in first.Keys)
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

    private async Task<string> GetCachedFilePathAsync(string fileName, string? checksum, CancellationToken token)
    {
        // Determine extension and cache file name
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext))
            ext = ".bin";

        var key = !string.IsNullOrEmpty(checksum) ? checksum : Path.GetFileNameWithoutExtension(fileName);
        var cached = Path.Combine(_cacheDir, key + ext);

        if (File.Exists(cached))
            return cached;

        // download
        var client = _httpFactory.CreateClient("gateway"); // optional named client
        var encoded = Uri.EscapeDataString(fileName);
        var url = GatewayBaseUrl + encoded;

        var tmp = Path.Combine(_cacheDir, Guid.NewGuid().ToString() + ext);
        using (var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token))
        {
            resp.EnsureSuccessStatusCode();
            using var src = await resp.Content.ReadAsStreamAsync(token);
            using var dst = File.Create(tmp);
            await src.CopyToAsync(dst, token);
        }

        // atomic move
        try
        {
            File.Move(tmp, cached);
        }
        catch
        {
            if (File.Exists(tmp))
            {
                try
                { File.Delete(tmp); }
                catch { }
            }
        }

        return cached;
    }
}
