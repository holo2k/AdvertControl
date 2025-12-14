using AdControl.ScreenClient.Services;

using System.Dynamic;
using System.Text.Json;
using AdControl.ScreenClient.Enums;
using AdControl.ScreenClient.Services;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AdControl.ScreenClient
{
    public partial class PlayerWindow : Window
    {
        private PlayerService _player;
        private List<ConfigItemDto> _items;
        private CancellationTokenSource _cts = new();
        public ConfigItemDto? CurrentItem { get; set; }
        private int startIndex;

        public PlayerWindow(List<ConfigItemDto>? items, int startIndex)
        {
            InitializeComponent(); 
            _items = items ?? new List<ConfigItemDto>();
            _player = new PlayerService(VideoViewControl, ImageControl, JsonTable);
            DataContext = this;
            this.startIndex = startIndex;
            _ = StartLoopAsync(_cts.Token);
        }

        public void UpdateItems(List<ConfigItemDto> items)
        {
            if (!_items.SequenceEqual(items))
            {
                _items = items;
                _cts.Cancel();
                _cts = new CancellationTokenSource();
            }
        }
        
        private async Task StartLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var snapshot = await Dispatcher.UIThread.InvokeAsync(() => _items.ToList());

                var i = startIndex;
                while (i < snapshot.Count)
                {
                    var item = snapshot[i];
                    
                    CurrentItem = item;

                    switch (item.Type)
                    {
                        case "Video":
                            await _player.ShowVideoAsync(item.Url, item.DurationSeconds, token);
                            break;

                        case "Image":
                            await _player.ShowImageAsync(item.Url, item.DurationSeconds, token, false);
                            break;

                        case "InlineJson":
                            var rows = await GetDynamicListFromJson(item.InlineData);
                            if (rows != null)
                                await _player.ShowTableAsync(rows, item.DurationSeconds, token);
                            break;
                    }

                    i++;
                    if (i == snapshot.Count) i = 0;
                }
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
    }
}