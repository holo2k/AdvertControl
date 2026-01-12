using AdControl.ScreenClient.Core.Options;
using AdControl.ScreenClient.Core.Services.Abstractions;

namespace AdControl.ScreenClient.Services
{
    public class FileCacheService : IFileCacheService
    {
        private readonly string _cacheDir;
        private const string GatewayBaseUrl = "https://advertcontrol.ru/files/";
        private readonly IHttpClientFactory _httpFactory;

        public FileCacheService(IAppPaths paths, IHttpClientFactory httpFactory)
        {
            _cacheDir = paths.CacheDir;
            _httpFactory = httpFactory;

            Directory.CreateDirectory(_cacheDir);
        }

        public async Task<string> GetCachedFilePathAsync(string fileName, string? checksum, CancellationToken token)
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

}
