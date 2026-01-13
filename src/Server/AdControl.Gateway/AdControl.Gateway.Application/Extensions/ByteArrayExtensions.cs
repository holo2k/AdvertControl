using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace AdControl.Gateway.Application.Extensions
{
    public static class ByteArrayExtensions
    {
        public static IFormFile ToFile(this byte[] bytes, string fileName = "file.jpg", string contentType = "image/jpeg")
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }

}
