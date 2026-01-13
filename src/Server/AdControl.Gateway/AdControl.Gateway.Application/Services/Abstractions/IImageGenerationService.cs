namespace AdControl.Gateway.Application.Services.Abstractions
{
    public interface IImageGenerationService
    {
        Task<string> GenerateImageAsync(string prompt, CancellationToken ct);
    }
}
