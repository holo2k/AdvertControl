using System.IO.Compression;
using AdControl.Protos;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly FileService.FileServiceClient _fileServiceClient;
    private readonly AuthService.AuthServiceClient _authServiceClient;

    public FileController(FileService.FileServiceClient fileServiceClient,
        AuthService.AuthServiceClient authServiceClient)
    {
        _fileServiceClient = fileServiceClient;
        _authServiceClient = authServiceClient;
    }

    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var currentUserRequest = new UserIdRequest { Token = token };
        var currentUser = _authServiceClient.GetCurrentUserId(currentUserRequest);
        var userId = currentUser.Id;

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var extension = Path.GetExtension(file.FileName);
        var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);

        var newFileName = $"{safeFileName}_{Guid.NewGuid().ToString().Substring(0,8)}_{userId}{extension}";

        var request = new UploadFileRequest
        {
            FileName = newFileName,
            FileData = ByteString.CopyFrom(ms.ToArray())
        };

        var resp = await _fileServiceClient.UploadFileAsync(request);
        return Ok(resp);
    }

    [HttpGet("get-current-user-files-name")]
    [Authorize]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserFilesName()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var currentUserRequest = new UserIdRequest { Token = token };
        var currentUser = await _authServiceClient.GetCurrentUserIdAsync(currentUserRequest);
        var userId = currentUser.Id;
        if (userId is null) throw new UnauthorizedAccessException();
        var request = new GetFilesNameByUserIdRequest
        {
            UserId = userId
        };
        var files = await _fileServiceClient.GetFilesNameByUserIdAsync(request);
        return Ok(files);
    }

    [HttpGet("get-current-user-files")]
    [Authorize]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUserFiles()
    {
        var tokenHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(tokenHeader)) return Unauthorized();
        var token = tokenHeader.Replace("Bearer ", "");

        var currentUserRequest = new UserIdRequest { Token = token };
        var currentUser = await _authServiceClient.GetCurrentUserIdAsync(currentUserRequest);
        var userId = currentUser?.Id;
        if (userId is null || userId == "") return Unauthorized();

        var namesRequest = new GetFilesNameByUserIdRequest
        {
            UserId = userId
        };
        var fileNamesResponse = await _fileServiceClient.GetFilesNameByUserIdAsync(namesRequest);
        var fileNames = fileNamesResponse.FilesName.ToList();
        if (fileNames.Count == 0) return NotFound("0 files");

        var filesRequest = new GetFilesByUserIdRequest { UserId = userId };
        var filesResponse = await _fileServiceClient.GetFilesByUserIdAsync(filesRequest);
        var files = filesResponse?.Files?.Select(x => x.ToByteArray()).ToList();
        if (files == null || files.Count == 0) return NotFound();

        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (var i = 0; i < files.Count; i++)
            {
                var entryName = (i < fileNames.Count && !string.IsNullOrWhiteSpace(fileNames[i]))
                    ? fileNames[i]
                    : $"file_{i + 1}.bin"; 

                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(files[i].AsMemory(0, files[i].Length));
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream, "application/zip", "all-files.zip");
    }

    [Obsolete("Метод устарел, используйте получение картинок через /img/image.png")]
    [HttpGet("{fileName}")]
    [Authorize]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string fileName)
    {
        var request = new GetFileRequest { FileName = fileName };
        var resp = await _fileServiceClient.GetFileAsync(request);

        if (resp?.FileData == null || resp.FileData.Length == 0)
            return NotFound();

        var bytes = resp.FileData.ToByteArray();

        var contentType = GetContentType(fileName);

        Response.Headers["Content-Disposition"] = "inline";

        return File(bytes, contentType);
    }

    [Obsolete("Метод устарел, используйте получение картинок через /img/image.png")]
    [HttpGet("by-url/{*url}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUrl(string url)
    {
        var decodedUrl = Uri.UnescapeDataString(url);
        var fileName = decodedUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

        if (string.IsNullOrEmpty(fileName))
            return NotFound();

        return await Get(fileName);
    }

    private static string GetContentType(string fileName)
    {
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fileName, out var contentType))
            contentType = "application/octet-stream";

        return contentType;
    }
}