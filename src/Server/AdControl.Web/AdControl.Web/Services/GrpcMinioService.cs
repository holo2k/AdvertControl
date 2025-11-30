using AdControl.Gateway.Application.Minio;
using Grpc.Core;
using AdControl.Protos;

public class GrpcMinioService : FileService.FileServiceBase
{
    private readonly MinioFileService _fileService;

    public GrpcMinioService(MinioFileService fileService)
    {
        _fileService = fileService;
    }

    public override async Task<UploadFileResponse> UploadFile(UploadFileRequest request, ServerCallContext context)
    {
        var url = await _fileService.UploadFileAsync(request.FileName, request.FileData.ToByteArray());
        return new UploadFileResponse { FileUrl = url };
    }

    public override async Task<GetFileResponse> GetFile(GetFileRequest request, ServerCallContext context)
    {
        var data = await _fileService.GetFileAsync(request.FileName);
        return new GetFileResponse { FileData = Google.Protobuf.ByteString.CopyFrom(data) };
    }

    public override async Task<GetFilesNameByUserIdResponse> GetFilesNameByUserId(GetFilesNameByUserIdRequest request,
        ServerCallContext context)
    {
        var data = await _fileService.GetFilesNameByUserAsync(request.UserId);
        return new GetFilesNameByUserIdResponse
        {
            FilesName = { data }
        };
    }

    public override async Task<GetFilesByUserIdResponse> GetFilesByUserId(GetFilesByUserIdRequest request,
        ServerCallContext context)
    {
        var data = (await _fileService.GetUserFilesContentAsync(request.UserId)).Select(Google.Protobuf.ByteString
            .CopyFrom);
        return new GetFilesByUserIdResponse
        {
            Files = { data }
        };
    }
}