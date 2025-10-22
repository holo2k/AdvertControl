using Minio;
using Microsoft.Extensions.Options;
using Minio.DataModel.Args;

namespace AdControl.Gateway.Application.Minio;

public class MinioFileService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private const string BucketName = "files";

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
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(BucketName)
            );
        }
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

        return $"http://{_settings.Endpoint}/{BucketName}/{fileName}";
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
}