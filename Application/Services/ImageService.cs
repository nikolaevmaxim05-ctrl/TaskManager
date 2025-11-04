    using TaskManager.Application.Interfaces;

    namespace TaskManager.Application.Services;

    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;

        public ImageService(ILogger<ImageService> logger)
        {
            _logger = logger;
        }
        public async Task<string> SaveImageAsync(IFormFile file, string saveDirectory, string? oldRelativePath = null)
        {
            _logger.LogInformation($"Попытка сохранить изображение {file.Name}");

            //проверка на валидность входных данных
            if (file == null || file.Length == 0)
            {
                _logger.LogError("При попытке сохранить изображение на вход методу поступил null объект");
                
                return oldRelativePath ?? string.Empty;
            }

            // Проверяем MIME-тип
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            if (!allowedTypes.Contains(file.ContentType))
            {
                _logger.LogError(
                    $"При попытке сохранения изображения на вход методу поступил файл с недопустимым типом {file.ContentType}");
                
                throw new InvalidOperationException("Неподдерживаемый формат изображения.");
            }

            // Создаём директорию, если её нет
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            // Удаляем старый файл (если был)
            if (!string.IsNullOrEmpty(oldRelativePath))
            {
                try
                {
                    var absoluteOldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        oldRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (File.Exists(absoluteOldPath))
                        File.Delete(absoluteOldPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"При попытке сохранения изображения возникло неизвестное исключение {ex.Message}");
                }
            }

            // Генерируем новое имя файла
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(saveDirectory, fileName);

            // Сохраняем файл
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 🔥 Преобразуем абсолютный путь в относительный к wwwroot
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativePath = fullPath.Replace(wwwrootPath, string.Empty)
                .Replace(Path.DirectorySeparatorChar, '/');

            // Убираем возможный двойной слэш в начале
            if (!relativePath.StartsWith("/"))
                relativePath = "/" + relativePath;

            return relativePath;
        }



        public async Task<IFormFile> ReadImageAsync(string relativePath, string webRootPath)
        {
            _logger.LogInformation($"Попытка считать изображение {relativePath}");

            //проверка входных данных на валидность
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                _logger.LogError("При попытке считывания изображение на вход методу поступил null объект");

                throw new ArgumentException("Путь к фото не указан.");
            }

            //собираем полный путь к фото
            var fullPath = Path.Combine(webRootPath, relativePath.TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar));

            //проверяем существует ли файл
            if (!File.Exists(fullPath))
            {
                _logger.LogError($"При попытке считать изображение на вход методу поступил путь по которому не был найден файл {relativePath}");
                
                throw new FileNotFoundException("Файл не найден", fullPath);
            }

            //считываем файл
            var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(fullPath));
            var fileName = Path.GetFileName(fullPath);
            var contentType = "image/" + Path.GetExtension(fileName).TrimStart('.');
            
            // заменяем все слэши для единообразия
            string normalized = fileName.Replace('\\', '/');
            
            return new FormFile(memoryStream, 0, memoryStream.Length, "photo", normalized)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        public async Task<bool> DeleteImageAsync(string relativePath, string webRootPath)
        {
            _logger.LogInformation($"Попытка удалить изображение {relativePath}");

            // проверка входных данных на null
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                _logger.LogError("При попытке удаления изображение на вход методу поступил null объект");

                return false;
            }

            //удаляем изображение
            try
            {
                var fullPath = Path.Combine(webRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
                else
                {
                    _logger.LogError($"При попытке считать изображение на вход методу поступил путь по которому не был найден файл {relativePath}");

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("При попытке удаления файла возникло неизвестное исключение");

                throw ex;
            }
        }

    }