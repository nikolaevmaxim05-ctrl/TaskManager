namespace TaskManager.Application.Interfaces;

public interface IImageService
{
    public Task<string> SaveImageAsync(IFormFile avatar, string webRootPath, string? oldAvatarPath);
    public Task<IFormFile> ReadImageAsync(string relativePath, string webRootPath);
    public Task<bool> DeleteImageAsync(string relativePath, string webRootPath);
}