using Moq;
using TaskManager.Application.Services;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class ImageServiceTests
{
    private readonly Mock<ILogger<ImageService>> _logger;
    private readonly ImageService _service;

    public ImageServiceTests()
    {
        _logger = new Mock<ILogger<ImageService>>();
        _service = new ImageService(_logger.Object);
    }

    private IFormFile CreateFormFile(string fileName, string contentType, string content = "fake image data")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
    [Fact]
    public async Task SaveImageAsync_ReturnsPath_WhenImageIsValid()
    {
        var tempDir = Directory.CreateTempSubdirectory("tempDirectory").FullName;

        var file = CreateFormFile("test.jpg", "image/jpeg");

        var result = await _service.SaveImageAsync(file, tempDir);

        Assert.NotNull(result);
        Assert.StartsWith("/", result);

        var savedFileName = result.TrimStart('/');
        var savedFilePath = Path.Combine(tempDir, savedFileName);

        Assert.True(File.Exists(savedFilePath));

        // Проверяем расширение
        Assert.EndsWith(".jpg", savedFileName);

        // Проверяем что имя файла — именно GUID
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(savedFileName);
        Assert.True(Guid.TryParse(fileNameWithoutExt, out _), "Файл должен иметь GUID в качестве имени");
    }

    [Fact]
    public async Task SaveImageAsync_ReturnsOldPath_WhenFileIsNull()
    {
        var tempDir = Directory.CreateTempSubdirectory("tempDirectory").FullName;
        var oldPath = "/uploads/old.jpg";

        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(0);
        var result = await _service.SaveImageAsync(file.Object, tempDir);
        
        Assert.False(File.Exists(Path.Combine(tempDir, "test.jpg")));
        Assert.Equal(string.Empty, result);
    }
    [Fact]
    public async Task SaveImageAsync_ThrowsException_WhenFileTypeInvalid()
    {
        var tempDir = Directory.CreateTempSubdirectory("tempDirectory").FullName;

        var file = CreateFormFile("test.exe", "application/octet-stream");
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _service
            .SaveImageAsync(file, tempDir, ""));
    }
    [Fact]
    public async Task SaveImageAsync_DeletesOldFile_WhenNewSaving()
    {
        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(wwwroot);

        var oldFileName = "old.jpg";
        var absoluteOldPath = Path.Combine(wwwroot, oldFileName);
        await File.WriteAllTextAsync(absoluteOldPath, "old file");
        
        var tempDir = Directory.CreateTempSubdirectory("tempDirectory").FullName;

        var newFile = CreateFormFile("new.jpg", "image/jpeg");

        await _service.SaveImageAsync(newFile, tempDir, $"/{oldFileName}");
        
        Assert.False(File.Exists("/old.jpg"));
    }

    [Fact]
    public async Task ReadImageAsync_ReturnsImage_WhenImageIsValid()
    {
        var file =  CreateFormFile("test.jpg", "image/jpg");
        var webRootPath = Directory.CreateTempSubdirectory("wwwroot").FullName;
        var fileName = Path.Combine(webRootPath, "test.jpg");
        await File.WriteAllBytesAsync(fileName, new  byte[] { 1, 2, 3 });

        var result = await _service.ReadImageAsync("/test.jpg", webRootPath);
        
        Assert.NotNull(result);
        Assert.Equal("test.jpg", result.FileName);
        Assert.Equal(file.ContentType, result.ContentType); 
    }
    [Fact]
    public async Task ReadImageAsync_ArgumentException_WhenImageIsNull()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await _service.ReadImageAsync(null, null));
    }
    [Fact]
    public async Task ReadImageAsync_FileNotFoundException_WhenImageIsNotExists()
    {
        var webRootPath = Directory.CreateTempSubdirectory("wwwroot").FullName;

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.ReadImageAsync("/test.jpg", webRootPath));
    }
    
    [Fact]
    public async Task DeleteImageAsync_ReturnsImage_WhenImageIsValid()
    {
        var webRootPath = Directory.CreateTempSubdirectory("wwwroot").FullName;
        var fileName = Path.Combine(webRootPath, "test.jpg");
        await File.WriteAllBytesAsync(fileName, new  byte[] { 1, 2, 3 });

        var result = await _service.DeleteImageAsync("/test.jpg", webRootPath);
        
        Assert.True(result);
        Assert.False(File.Exists(Path.Combine(webRootPath, "test.jpg")));
    }
    [Fact]
    public async Task DeleteImageAsync_False_WhenImageIsNull()
    {
        var webRootPath = Directory.CreateTempSubdirectory("wwwroot").FullName;
        
        var result =  await _service.DeleteImageAsync(null, webRootPath);
        
        Assert.False(result);
    }
    [Fact]
    public async Task DeleteImageAsync_False_WhenImageIsNotExists()
    {
        var webRootPath = Directory.CreateTempSubdirectory("wwwroot").FullName;
        var fileName = "test.jpg";
        
        var result = await _service.DeleteImageAsync(fileName, webRootPath);
        
        Assert.False(result);
    }

}

