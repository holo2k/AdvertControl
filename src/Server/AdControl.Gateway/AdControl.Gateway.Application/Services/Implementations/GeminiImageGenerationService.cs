using System.Text;
using System.Text.Json;
using AdControl.Gateway.Application.Services.Abstractions;

namespace AdControl.Gateway.Application.Services.Implementations
{
    public sealed class GeminiImageGenerationService : IImageGenerationService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private const string _openRouterUrl = "https://openrouter.ai/api/v1/chat/completions";

        public GeminiImageGenerationService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
                      ?? throw new InvalidOperationException("OPENROUTER_API_KEY environment variable is not set");
        }

        public async Task<string> GenerateImageAsync(string prompt, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("prompt is empty", nameof(prompt));
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("OPENROUTER_API_KEY is not set");

            var requestBody = new
            {
                model = "bytedance-seed/seedream-4.5",
                messages = new[]
                {
                    new { role = "user", content = new[] { new { type = "text", text = prompt } } }
                },
                max_tokens = 1000,
                temperature = 0.7,
                top_p = 1
            };

            var responseText = await SendOpenRouterRequestAsync(requestBody, ct);

            using var doc = JsonDocument.Parse(responseText);

            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
                throw new Exception("OpenRouter response missing choices. Full response: " + responseText);

            // Пробуем найти image_base64 в нескольких местах, возвращаем первый найденный валидный
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.ValueKind != JsonValueKind.Object)
                    continue;

                // 1) message.content[..] вариант (image_base64 / image_base64)
                if (choice.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.Object)
                {
                    if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in content.EnumerateArray())
                        {
                            // формат: { "type":"image", "image_base64": "..." }
                            if (TryGetString(item, "image_base64", out var base64FromContent) && !string.IsNullOrWhiteSpace(base64FromContent))
                            {
                                ValidateBase64OrThrow(base64FromContent, responseText);
                                return base64FromContent;
                            }

                            // альтернативный ключ
                            if (TryGetString(item, "image_base64_encoded", out var altBase64) && !string.IsNullOrWhiteSpace(altBase64))
                            {
                                ValidateBase64OrThrow(altBase64, responseText);
                                return altBase64;
                            }

                            // возможен data-url внутри content.url
                            if (TryGetString(item, "url", out var urlInContent) && !string.IsNullOrWhiteSpace(urlInContent))
                            {
                                var b = await ExtractBase64FromUrlOrDataUrlAsync(urlInContent, ct);
                                if (!string.IsNullOrWhiteSpace(b))
                                {
                                    ValidateBase64OrThrow(b, responseText);
                                    return b;
                                }
                            }
                        }
                    }

                    // 2) message.images[..] вариант
                    if (message.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var img in images.EnumerateArray())
                        {
                            // ожидается img.image_url.url
                            if (img.TryGetProperty("image_url", out var imageUrlObj) && imageUrlObj.ValueKind == JsonValueKind.Object)
                            {
                                if (imageUrlObj.TryGetProperty("url", out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                                {
                                    var url = urlEl.GetString();
                                    var b = await ExtractBase64FromUrlOrDataUrlAsync(url, ct);
                                    if (!string.IsNullOrWhiteSpace(b))
                                    {
                                        ValidateBase64OrThrow(b, responseText);
                                        return b;
                                    }
                                }
                            }

                            // альтернативное поле: img.url
                            if (TryGetString(img, "url", out var directUrl) && !string.IsNullOrWhiteSpace(directUrl))
                            {
                                var b = await ExtractBase64FromUrlOrDataUrlAsync(directUrl, ct);
                                if (!string.IsNullOrWhiteSpace(b))
                                {
                                    ValidateBase64OrThrow(b, responseText);
                                    return b;
                                }
                            }
                        }
                    }
                }
            }

            // ничего не нашли
            throw new Exception("OpenRouter did not return image data. Full response: " + responseText);
        }

        private async Task<string> SendOpenRouterRequestAsync(object requestBody, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, _openRouterUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            using var response = await _http.SendAsync(request, ct);
            var responseJson = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter API Error: {response.StatusCode}, {responseJson}");

            return responseJson;
        }

        private static bool TryGetString(JsonElement el, string propName, out string? value)
        {
            value = null;
            if (el.ValueKind != JsonValueKind.Object)
                return false;
            if (!el.TryGetProperty(propName, out var p))
                return false;
            if (p.ValueKind != JsonValueKind.String)
                return false;
            value = p.GetString();
            return true;
        }

        private static void ValidateBase64OrThrow(string base64, string fullResponse)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                if (bytes.Length < 4)
                    throw new FormatException();
            }
            catch (Exception ex)
            {
                throw new Exception("Extracted string is not valid Base64 image. Full response: " + fullResponse, ex);
            }
        }

        private async Task<string?> ExtractBase64FromUrlOrDataUrlAsync(string? url, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // data:[<mediatype>][;base64],<data>
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var comma = url.IndexOf(',');
                if (comma < 0)
                    return null;
                var payload = url[(comma + 1)..];
                return payload;
            }

            // Если вернулся внешний URL, загрузим ресурс и вернём Base64
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var bytes = await _http.GetByteArrayAsync(new Uri(url), ct);
                    return Convert.ToBase64String(bytes);
                }
                catch
                {
                    return null; // не ломаем основной поток, пробуем другие варианты
                }
            }

            return null;
        }
    }
}
