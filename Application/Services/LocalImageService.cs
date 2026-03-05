    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using TaskManager.Application.Interfaces;

    namespace TaskManager.Application.Services;

    public class LocalImageService : IImageService
    {
        private readonly ILogger<LocalImageService> _logger;
        private readonly IWebHostEnvironment _env;

        public LocalImageService(ILogger<LocalImageService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
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
            // создаем уникальное имя
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // путь к папке
            var imagesFolder = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            var filePath = Path.Combine(imagesFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation($"Изображение сохранено {fileName}");

            // возвращаем URL путь
            return $"/images/{fileName}";
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            _logger.LogInformation($"Попытка удалить изображение {imageUrl}");

            // проверка входных данных на null
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogError("При попытке удаления изображение на вход методу поступил null объект");

                return false;
            }

            try
            {
                // получаем имя файла
                var fileName = Path.GetFileName(imageUrl);

                // путь к папке images
                var imagesFolder = Path.Combine(_env.WebRootPath, "images");

                // полный путь
                var filePath = Path.Combine(imagesFolder, fileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"Файл не найден {filePath}");
                    return await Task.FromResult(false);
                }

                File.Delete(filePath);

                _logger.LogInformation($"Файл успешно удален {fileName}");

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении изображения");
                return await Task.FromResult(false);
            }
        }

    }