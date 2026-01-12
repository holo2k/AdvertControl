using AdControl.ScreenClient.Core.Services;
using AdControl.ScreenClient.Core.Services.Abstractions;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AdControl.ScreenClient
{
    public partial class PlayerWindow : Window
    {
        private PlayerService _player;
        private List<ConfigItemDto> _items;
        private CancellationTokenSource _cts = new();
        private bool isStatic;
        public ConfigItemDto? CurrentItem { get; set; }
        private int startIndex;

        public PlayerWindow(List<ConfigItemDto>? items, int startIndex, bool isStatic = false)
        {
            InitializeComponent();
            _items = items ?? new List<ConfigItemDto>();

            var httpFactory = App.Services?.GetService<IHttpClientFactory>()
                      ?? throw new InvalidOperationException("IHttpClientFactory not registered in DI");

            var fileCacheService = App.Services?.GetService<IFileCacheService>()
                      ?? throw new InvalidOperationException("IFileCacheService not registered in DI");

            _player = new PlayerService(VideoViewControl, ImageControl, JsonTable, httpFactory, fileCacheService);

            DataContext = this;

            // normalize start index to [0, count-1]
            if (_items.Count > 0)
            {
                var idx = startIndex % _items.Count;
                if (idx < 0)
                    idx += _items.Count;
                this.startIndex = idx;
            }
            else
            {
                this.startIndex = 0;
            }

            this.isStatic = isStatic;
            _ = StartLoopAsync(_cts.Token);
        }

        public void UpdateItems(List<ConfigItemDto> items)
        {
            if (!_items.SequenceEqual(items))
            {
                _items = items;

                // нормализуем стартовый индекс в новую длину
                if (_items.Count == 0)
                    startIndex = 0;
                else
                    startIndex = startIndex % _items.Count;

                // перезапускаем цикл плеера с новым токеном
                _cts.Cancel();
                _cts = new CancellationTokenSource();
                _ = StartLoopAsync(_cts.Token);
            }
        }

        private async Task StartLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var snapshot = await Dispatcher.UIThread.InvokeAsync(() => _items.ToList());

                var i = startIndex;

                while (!token.IsCancellationRequested && snapshot.Count > 0)
                {
                    var item = snapshot[i];

                    CurrentItem = item;

                    switch (item.Type)
                    {
                        case "Video":
                            await _player.ShowVideoAsync(item, token);
                            break;
                        case "Image":
                            await _player.ShowImageAsync(item, token);
                            break;
                        case "InlineJson":
                            var rows = await ParsingHelper.GetDynamicListFromJson(item.InlineData);
                            if (rows != null)
                                await _player.ShowTableAsync(rows, item.DurationSeconds, token);
                            break;
                    }

                    if (!isStatic)
                    {
                        i = (i + 1) % snapshot.Count;
                    }
                }
            }
        }
    }
}