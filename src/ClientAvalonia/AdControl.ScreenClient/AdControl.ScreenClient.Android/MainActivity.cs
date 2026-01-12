using System.Dynamic;
using System.Xml;
using AdControl.ScreenClient.Android.Services;
using AdControl.ScreenClient.Core.Options;
using AdControl.ScreenClient.Core.Services;
using AdControl.ScreenClient.Core.Services.Abstractions;
using Android.Views;
using Graphics = Android.Graphics;
using OperationCanceledException = Android.OS.OperationCanceledException;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AdControl.ScreenClient.Android
{
    [Activity(
        Name = "com.swaga.advertcontrol.MainActivity",
        Label = "@string/app_name",
        MainLauncher = true,
        Exported = true,
        Theme = "@style/Theme.AppCompat.NoActionBar"
    )]
    public class MainActivity : Activity
    {
        private VideoView _videoView;
        private ImageView _imageView;
        private TableLayout _tableLayout;
        private ScrollView _tableScroll;
        private TextView _statusText, _pairCode;
        private ImageView _qrImage;

        private IFileCacheService _fileCache;
        private PollingService _polling;
        private AndroidPlayerService _player;
        private IQrGenerator _qrGenerator;

        private CancellationTokenSource _cts = new();

        // Временные переменные состояния
        private string _screenId = string.Empty;
        private long _knownVersion = -1;
        private bool _isStatic = false;
        private int _intervalSeconds = 5;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _videoView = FindViewById<VideoView>(Resource.Id.videoView);
            _imageView = FindViewById<ImageView>(Resource.Id.imageView);
            _tableLayout = FindViewById<TableLayout>(Resource.Id.tableLayout);
            _tableScroll = FindViewById<ScrollView>(Resource.Id.tableScroll);
            _statusText = FindViewById<TextView>(Resource.Id.statusText);
            _pairCode = FindViewById<TextView>(Resource.Id.pairCode);
            _qrImage = FindViewById<ImageView>(Resource.Id.qrImage);

            EnsureAppSettingsExists();

            // Инициализация сервисов
            var http = new HttpClient { BaseAddress = new Uri("https://advertcontrol.ru") };

            var gatewayUrl = GetGatewayUrlFromSettings();
            var httpClient = new HttpClient { BaseAddress = new Uri(gatewayUrl), Timeout = TimeSpan.FromSeconds(10) };
            var factory = new AndroidHttpClientFactory(httpClient);
            _polling = new PollingService(factory);
            _fileCache = new AndroidFileCacheService(new AndroidAppPaths(), http);
            _qrGenerator = new ZxingQrGenerator();

            _player = new AndroidPlayerService(this, _videoView, _imageView, _tableLayout, _fileCache);

            // прочитать screenId из appsettings.json (если есть)
            _screenId = ReadScreenIdFromAppSettings();

            // показать статус
            RunOnUiThread(() =>
            {
                _statusText.Text = string.IsNullOrWhiteSpace(_screenId)
                    ? "ID экрана не установлен. Используйте привязку."
                    : $"ID={_screenId}";
                // начальное состояние — если есть id — будет проверено в инициализации
                SetState(string.IsNullOrWhiteSpace(_screenId) ? ScreenState.NotPaired : ScreenState.Pairing);
            });

            // Запуск инициализации/цикла
            _ = InitializeAsync(_cts.Token);
        }

        private async Task InitializeAsync(CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(_screenId))
            {
                await CheckScreenExistAndAdjustAsync(_screenId, token);
                if (!string.IsNullOrWhiteSpace(_screenId))
                {
                    _ = StartAsync(token);
                    return;
                }
            }

            // запуск цикла привязки если не привязан
            _ = StartPairingLoopAsync(token);
        }

        private enum ScreenState
        {
            NotPaired,
            Pairing,
            Paired
        }

        private ScreenState _state = ScreenState.NotPaired;

        private void SetState(ScreenState state)
        {
            _state = state;
            RunOnUiThread(() =>
            {
                switch (state)
                {
                    case ScreenState.Paired:
                    _pairCode.Visibility = ViewStates.Gone;
                    _qrImage.Visibility = ViewStates.Gone;
                    _tableScroll.Visibility = ViewStates.Gone;
                    break;
                    case ScreenState.NotPaired:
                    case ScreenState.Pairing:
                    _pairCode.Visibility = ViewStates.Visible;
                    _qrImage.Visibility = ViewStates.Visible;
                    break;
                }
            });
        }

        // Парсер inline json -> List<ExpandoObject>
        private List<ExpandoObject>? ParseInlineJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var rows = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
            if (rows == null || rows.Count == 0)
                return null;

            var list = new List<ExpandoObject>();
            foreach (var dict in rows)
            {
                var exp = new ExpandoObject() as IDictionary<string, object?>;
                foreach (var pair in dict)
                    exp[pair.Key] = pair.Value;
                list.Add((ExpandoObject)exp);
            }
            return list;
        }


        // Основной цикл показа (аналог StartLoopAsync + ShowItemsAsync)
        public async Task StartAsync(CancellationToken token)
        {
            var showTask = Task.Run(() => ShowItemsLoop(token), token);
            var pollTask = Task.Run(() => StartLoopAsync(token), token);
            await Task.WhenAll(showTask, pollTask);
        }

        // Цикл poll (обновление конфигурации)
        private async Task StartLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_state != ScreenState.Paired)
                    {
                        await Task.Delay(500, token);
                        continue;
                    }

                    await PollOnce(token);
                    if (DateTime.UtcNow.Minute % 5 == 0)
                    {
                        await CheckScreenExistAndAdjustAsync(_screenId, token);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                RunOnUiThread(() => _statusText.Text = $"Ошибка в цикле poll: {ex.Message}");
            }
        }

        private async Task PollOnce(CancellationToken token)
        {
            try
            {
                RunOnUiThread(() => _statusText.Text = "Обращение к серверу...");
                var cfg = await _polling.GetConfigAsync(_screenId, _knownVersion);

                if (cfg == null)
                    throw new Exception("Конфиг пуст либо не загружен :(");

                _isStatic = cfg.isStatic;
                _knownVersion = cfg.Version;

                if (cfg.NotModified)
                    return;

                // Обновляем items — для простоты здесь мы не храним Items коллекцию, а передаём cfg в ShowItemsLoop
                // Сохраняем cfg в поле для считывания show loop'ом
                _lastConfig = cfg;

                RunOnUiThread(() => _statusText.Text = string.Empty);
            }
            catch (Exception ex)
            {
                RunOnUiThread(() => _statusText.Text = $"Ошибка poll: {ex.Message}");
            }
        }

        // буфер для последнего полученного конфига
        private ConfigDto? _lastConfig;

        // Показываем элементы из _lastConfig
        private async Task ShowItemsLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_state != ScreenState.Paired)
                {
                    await Task.Delay(200, token);
                    continue;
                }

                var cfg = _lastConfig;
                if (cfg is null || cfg.NotModified)
                {
                    await Task.Delay(200, token);
                    continue;
                }

                var items = cfg.Items.ToList();

                foreach (var item in items)
                {
                    if (token.IsCancellationRequested)
                        break;

                    RunOnUiThread(() =>
                    {
                        _statusText.Text = string.Empty;
                        _qrImage.Visibility = ViewStates.Gone;
                        _pairCode.Visibility = ViewStates.Gone;
                    });

                    switch (item.Type)
                    {
                        case "Video":
                        await _player.ShowVideoAsync(item, token);
                        break;
                        case "Image":
                        await _player.ShowImageAsync(item, token);
                        break;
                        case "InlineJson":
                        var rows = ParseInlineJson(item.InlineData);
                        if (rows != null)
                            await _player.ShowTableAsync(rows, item.DurationSeconds, token);
                        break;
                    }

                    if (_isStatic)
                        break;
                }

                await Task.Delay(200, token);
            }
        }

        // Цикл привязки (генерация кода, показ QR, ожидание присоединения)
        private async Task StartPairingLoopAsync(CancellationToken token)
        {
            SetState(ScreenState.Pairing);

            while (!token.IsCancellationRequested)
            {
                var code = new Random().Next(0, 1000000).ToString("D6");

                RunOnUiThread(() =>
                {
                    _statusText.Text = "Введите код по ссылке\nadvertcontrol.ru/screens";
                });

                UpdateCodeAndQr(code);

                try
                {
                    var tempId = Guid.NewGuid().ToString("N");
                    var started = await _polling.StartPairAsync(tempId, code, 5, "auto");
                    if (started)
                    {
                        var timeout = TimeSpan.FromMinutes(5);
                        var sw = System.Diagnostics.Stopwatch.StartNew();

                        while (sw.Elapsed < timeout && !token.IsCancellationRequested)
                        {
                            var (assigned, assignedScreenId) = await _polling.CheckPairStatusAsync(tempId);
                            if (assigned)
                            {
                                _screenId = assignedScreenId ?? string.Empty;
                                await SaveScreenIdToAppSettingsAsync(_screenId);
                                RunOnUiThread(() => _statusText.Text = $"Экран привязан. ID={_screenId}");
                                SetState(ScreenState.Paired);
                                _ = StartAsync(token);
                                return;
                            }

                            await Task.Delay(2000, token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => _statusText.Text = $"Ошибка привязки: {ex.Message}");
                }

                // Ждём 5 минут перед новой попыткой, как в desktop-логике
                await Task.Delay(TimeSpan.FromMinutes(5), token);
            }
        }

        private async Task CheckScreenExistAndAdjustAsync(string screenId, CancellationToken token)
        {
            try
            {
                var exists = await _polling.IsScreenExistAsync(screenId);
                if (exists is false)
                {
                    RunOnUiThread(() => _statusText.Text = "ID не найден. Очистка и переход в режим привязки.");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    await DeleteScreenIdAsync();
                    SetState(ScreenState.NotPaired);
                }
                else
                {
                    SetState(ScreenState.Paired);
                }
            }
            catch
            {
                // ignore
            }
        }

        private string ReadScreenIdFromAppSettings()
        {
            try
            {
                var node = LoadAppSettings();
                if (node["Screen"] is JObject scr && scr["Id"] != null)
                    return scr["Id"]!.ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task SaveScreenIdToAppSettingsAsync(string screenId)
        {
            try
            {
                var node = LoadAppSettings();
                if (node["Screen"] is not JObject screenObj)
                {
                    screenObj = new JObject();
                    node["Screen"] = screenObj;
                }
                screenObj["Id"] = screenId;
                await SaveAppSettingsAsync(node);
            }
            catch (Exception ex)
            {
                RunOnUiThread(() => _statusText.Text = $"Ошибка сохранения ID: {ex.Message}");
            }
        }

        private async Task DeleteScreenIdAsync()
        {
            try
            {
                var path = AppSettingsPath();
                if (!File.Exists(path))
                    return;

                var text = await File.ReadAllTextAsync(path);
                JObject node;
                try
                { node = JObject.Parse(text); }
                catch { node = new JObject(); }

                if (node["Screen"] is not JObject screenObj)
                {
                    screenObj = new JObject();
                    node["Screen"] = screenObj;
                }

                screenObj["Id"] = string.Empty;
                await File.WriteAllTextAsync(path, node.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch
            {
                // ignore
            }
        }


        // Генерация QR и установка изображения + текст кода
        private void UpdateCodeAndQr(string code)
        {
            RunOnUiThread(() =>
            {
                _pairCode.Text = code;
                _pairCode.Visibility = ViewStates.Visible;
            });

            _ = GenerateAndSetQrImageAsync(code, _cts.Token);
        }

        private async Task GenerateAndSetQrImageAsync(string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                RunOnUiThread(() => _qrImage.SetImageBitmap(null));
                return;
            }

            var url = $"https://advertcontrol.ru/screens/create-screen?code={Uri.EscapeDataString(code)}";

            try
            {
                var png = await Task.Run(() => _qrGenerator.GeneratePng(url), cancellationToken);
                var bmp = Graphics.BitmapFactory.DecodeByteArray(png, 0, png.Length);
                RunOnUiThread(() =>
                {
                    _qrImage.SetImageBitmap(bmp);
                    _qrImage.Visibility = ViewStates.Visible;
                });
            }
            catch (OperationCanceledException) 
            { 

            }
            catch (Exception)
            {
                RunOnUiThread(() => _qrImage.Visibility = ViewStates.Gone);
            }
        }

        protected override void OnDestroy()
        {
            _cts.Cancel();
            try
            { _player?.Dispose(); }
            catch { }
            base.OnDestroy();
        }

        static readonly SemaphoreSlim _appSettingsLock = new SemaphoreSlim(1, 1);

        string AppSettingsPath() =>
            Path.Combine(FilesDir.AbsolutePath, "appsettings.json");

        void EnsureAppSettingsExists()
        {
            var dst = AppSettingsPath();
            if (System.IO.File.Exists(dst))
                return;

            try
            {
                using var asset = Assets.Open("appsettings.json"); // файл в Assets/
                using var dstStream = System.IO.File.Create(dst);
                asset.CopyTo(dstStream);
            }
            catch (Exception ex)
            {
                // логирование, если нужно
                System.Diagnostics.Debug.WriteLine("EnsureAppSettingsExists error: " + ex.Message);
            }
        }

        private JObject LoadAppSettings()
        {
            try
            {
                var path = Path.Combine(FilesDir.AbsolutePath, "appsettings.json");
                if (!File.Exists(path))
                    return new JObject();

                var text = File.ReadAllText(path);
                return JObject.Parse(text);
            }
            catch
            {
                return new JObject();
            }
        }

        private async Task SaveAppSettingsAsync(JObject node)
        {
            await _appSettingsLock.WaitAsync();
            try
            {
                var path = Path.Combine(FilesDir.AbsolutePath, "appsettings.json");
                var text = node.ToString(Newtonsoft.Json.Formatting.Indented);
                await File.WriteAllTextAsync(path, text);
            }
            finally
            {
                _appSettingsLock.Release();
            }
        }

        string GetGatewayUrlFromSettings()
        {
            var node = LoadAppSettings();
            // безопасно получить значение (JsonNode.ToString() вернёт JSON-представление)
            var url = node["Gateway"]?["Url"]?.ToString()?.Trim('"');
            return string.IsNullOrWhiteSpace(url) ? "https://advertcontrol.ru" : url;
        }

        int GetPollingIntervalFromSettings()
        {
            var node = LoadAppSettings();
            var raw = node["Polling"]?["IntervalSeconds"]?.ToString();
            if (int.TryParse(raw, out var n))
                return n;
            return 5;
        }
    }
}
