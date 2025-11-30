using Microsoft.Extensions.Options;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;

namespace AdControl.Gateway.Application.Minio;

public class MinioFileService
{
    private const string BucketName = "files";
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;

    public MinioFileService(IOptions<MinioSettings> settings)
    {
        _settings = settings.Value;
        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.Secure)
            .Build();

        EnsureBucketExists().GetAwaiter().GetResult();
    }

    private async Task EnsureBucketExists()
    {
        var exists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName)
        );

        if (!exists)
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(BucketName)
            );
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] data)
    {
        using var ms = new MemoryStream(data);
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(fileName)
            .WithStreamData(ms)
            .WithObjectSize(ms.Length)
            .WithContentType("application/octet-stream"));

        return $"{fileName}";
        //return $"http://{_settings.Endpoint}/{BucketName}/{fileName}";
    }

    public async Task<byte[]> GetFileAsync(string fileName)
    {
        using var ms = new MemoryStream();
        await _minioClient.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream => stream.CopyTo(ms))
        );
        return ms.ToArray();
    }
    
    public async Task<List<string>> GetFilesNameByUserAsync(string userId)
    {
        var files = new List<string>();
        var tcs = new TaskCompletionSource<bool>();

        var observable = _minioClient.ListObjectsAsync(
            new ListObjectsArgs()
                .WithBucket(BucketName)
                .WithRecursive(true)
        );

        observable.Subscribe(
            item =>
            {
                if (item.Key.Contains($"_{userId}"))
                {
                    files.Add(item.Key);
                }
            },
            ex =>
            {
                tcs.SetException(ex);
            },
            () =>
            {
                tcs.SetResult(true);
            });

        await tcs.Task;
        return files;
    }
    
    public async Task<List<byte[]>> GetUserFilesContentAsync(string userId)
    {
        var filesContent = new List<byte[]>();
        var tcs = new TaskCompletionSource<bool>();

        var observable = _minioClient.ListObjectsAsync(
            new ListObjectsArgs()
                .WithBucket(BucketName)
                .WithRecursive(true)
        );

        observable.Subscribe(
            async item =>
            {
                if (item.Key.Contains($"_{userId}"))
                {
                    using var ms = new MemoryStream();
                    await _minioClient.GetObjectAsync(
                        new GetObjectArgs()
                            .WithBucket(BucketName)
                            .WithObject(item.Key)
                            .WithCallbackStream(stream => stream.CopyTo(ms))
                    );
                    filesContent.Add(ms.ToArray());
                }
            },
            ex =>
            {
                tcs.SetException(ex);
            },
            () =>
            {
                tcs.SetResult(true);
            });

        await tcs.Task;
        return filesContent;
    }

}