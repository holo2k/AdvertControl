using AdControl.Gateway.Application.Dtos;
using AdControl.Gateway.Application.Services.Abstractions;
using AdControl.Protos;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public ImageGenerationController(
            IImageGenerationService imageService,
            FileService.FileServiceClient fileServiceClient,
            AuthService.AuthServiceClient authServiceClient)
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

            if (bytes.Length == 0)
                return BadRequest("Generated image is empty.");

            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var currentUser = _authServiceClient.GetCurrentUserId(new UserIdRequest { Token = token });
            var userId = currentUser.Id;

            var fileName = $"generated_{Guid.NewGuid():N}_{userId}.jpg";

            var uploadRequest = new UploadFileRequest
            {
                FileName = fileName,
                FileData = ByteString.CopyFrom(bytes)
            };

            var resp = await _fileServiceClient.UploadFileAsync(uploadRequest);

            return Ok(resp);
        }
    }
}
