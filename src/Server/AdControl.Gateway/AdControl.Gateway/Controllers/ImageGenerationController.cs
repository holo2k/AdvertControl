using AdControl.Gateway.Application.Dtos;
using AdControl.Gateway.Application.Extensions;
using AdControl.Gateway.Application.Services.Abstractions;
using AdControl.Protos;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AdControl.Protos.AuthService;
using static AdControl.Protos.FileService;

namespace AdControl.Gateway.Controllers
{
    [ApiController]
    [Route("api/image")]
    [Produces("application/json")]
    public class ImageGenerationController : ControllerBase
    {
        private readonly IImageGenerationService _imageService;
        private readonly FileService.FileServiceClient _fileServiceClient;
        private readonly AuthService.AuthServiceClient _authServiceClient;

        public ImageGenerationController(IImageGenerationService imageService, FileServiceClient fileServiceClient, AuthServiceClient authServiceClient)
        {
            _imageService = imageService;
            _fileServiceClient = fileServiceClient;
            _authServiceClient = authServiceClient;
        }

        /// <summary>
        /// Генерирует изображение по текстовому описанию и загружает в MinIO.
        /// </summary>
        [HttpPost("generate")]
        [Authorize]
        [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Generate([FromBody] GenerateImageRequest request, CancellationToken ct)
        {
            var base64 = await _imageService.GenerateImageAsync(request.Prompt, ct);
            var bytes = Convert.FromBase64String(base64);

            var fileName = $"generated-{Guid.NewGuid().ToString().Substring(0, 8)}.jpg";
            var file = bytes.ToFile(fileName);

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

            var newFileName = $"{safeFileName}_{Guid.NewGuid().ToString().Substring(0, 8)}_{userId}{extension}";

            var uploadFileRequest = new UploadFileRequest
            {
                FileName = newFileName,
                FileData = ByteString.CopyFrom(ms.ToArray())
            };

            var resp = await _fileServiceClient.UploadFileAsync(uploadFileRequest);

            return Ok(resp);
        }
    }
}
