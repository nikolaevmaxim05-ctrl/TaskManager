namespace TaskManager.Application.Interfaces;

public interface IImageService
{
    public Task<string> SaveImageAsync(IFormFile avatar);
    public Task<bool> DeleteImageAsync(string webRootPath);
}