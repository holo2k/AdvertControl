using AdControl.ScreenClient.Services;

using System.Dynamic;
using System.Text.Json;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;

namespace AdControl.ScreenClient
{
    public partial class PlayerWindow : MainWindow
    {
        private readonly PlayerService _player;
        private readonly List<ConfigItemDto> _items;

        public PlayerWindow(List<ConfigItemDto>? items)
        {
            InitializeComponent(); 
            _items = items ?? new List<ConfigItemDto>();
            _player = new PlayerService(VideoViewControl, ImageControl, JsonTable);
            DataContext = this;
            _ = StartAsync();
        }

        private async Task StartAsync()
        {
            foreach (var item in _items)
            {
                switch (item.Type)
                {
                    case "Video":
                        await _player.ShowVideoAsync(item.Url, item.DurationSeconds, CancellationToken.None);
                        break;
                    case "Image":
                        await _player.ShowImageAsync(item.Url, item.DurationSeconds, CancellationToken.None);
                        break;
                    case "InlineJson":
                        var rows = JsonSerializer.Deserialize<List<ExpandoObject>>(item.InlineData);
                        if (rows != null)
                            await _player.ShowTableAsync(rows, item.DurationSeconds, CancellationToken.None);
                        break;
                }
            }
        }
    }
}