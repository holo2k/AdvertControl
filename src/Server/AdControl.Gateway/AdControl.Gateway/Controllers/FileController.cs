using AdControl.Protos;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly FileService.FileServiceClient _fileServiceClient;

    public FileController(FileService.FileServiceClient fileServiceClient)
    {
        _fileServiceClient = fileServiceClient;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var request = new UploadFileRequest
        {
            FileName = file.FileName,
            FileData = ByteString.CopyFrom(ms.ToArray())
        };

        var resp = await _fileServiceClient.UploadFileAsync(request);
        return Ok(resp);
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> Get(string fileName)
    {
        var request = new GetFileRequest { FileName = fileName };
        var resp = await _fileServiceClient.GetFileAsync(request);

        return File(resp.FileData.ToByteArray(), "application/octet-stream", fileName);
    }

    [HttpGet("by-url/{url}")]
    public async Task<IActionResult> GetByUrl(string url)
    {
        var decodedUrl = Uri.UnescapeDataString(url);

        var fileName = decodedUrl.Split('/').LastOrDefault();

        return await Get(fileName);
    }
}