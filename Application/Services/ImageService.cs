    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using TaskManager.Application.Interfaces;

    namespace TaskManager.Application.Services;

    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly BlobContainerClient _container;

        public ImageService(ILogger<ImageService> logger, IConfiguration config)
        {
            _logger = logger;
            
            var connectionString = config["AzureBlobConnectionString"];
            var containerName = "avatars";
            _container =  new BlobContainerClient(connectionString, containerName);
        }
        public async Task<string> SaveImageAsync(IFormFile file)
        {
            _logger.LogInformation($"Попытка сохранить изображение {file.Name}");

            //проверка на валидность входных данных
            if (file == null || file.Length == 0)
            {
                _logger.LogError("При попытке сохранить изображение на вход методу поступил null объект");
                
                return null;
            }

            // Проверяем MIME-тип
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            if (!allowedTypes.Contains(file.ContentType))
            {
                _logger.LogError(
                    $"При попытке сохранения изображения на вход методу поступил файл с недопустимым типом {file.ContentType}");
                
                throw new InvalidOperationException("Неподдерживаемый формат изображения.");
            }

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = _container.GetBlobClient(blobName);
            
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            await blobClient.UploadAsync(
                ms,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                });
            
            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteImageAsync(string webRootPath)
        {
            _logger.LogInformation($"Попытка удалить изображение {webRootPath}");

            // проверка входных данных на null
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                _logger.LogError("При попытке удаления изображение на вход методу поступил null объект");

                return false;
            }

            //удаляем изображение
            var uri = new Uri(webRootPath);
            
            var blobName = Path.GetFileName(uri.LocalPath);
            
            var blobClient = _container.GetBlobClient(blobName);
            
            return await blobClient.DeleteIfExistsAsync();  
        }

    }