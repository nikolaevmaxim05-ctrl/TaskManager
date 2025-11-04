using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class NoteService : INoteService
{
    private readonly ILogger<NoteService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IImageService _imageService;
    private readonly IWebHostEnvironment _environment;

    public NoteService(ILogger<NoteService> logger, IUserRepository userRepository, INoteRepository noteRepository, 
        IImageService imageService, IWebHostEnvironment environment)
    {
        _logger = logger;
        _userRepository = userRepository;
        _noteRepository = noteRepository;
        _imageService = imageService;
        _environment = environment;
    }
    /// <summary>
    /// Метод для получения всех задач авторизованного пользователя
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<List<NoteReadDto>> GetAllMyNoteAsync(HttpContext context)
    {
        _logger.LogInformation("Попытка получения всех задач авторизованного пользователя");
        
        //получение авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
        
        //получения всех задач авторизованного пользователя и маппинг в ReadDto
        var notePool = new List<NoteReadDto>();
        foreach (var task in user.NotePool)
        {
            notePool.Add(task.ToReadDto());
        }
        
        return notePool;
    }

    /// <summary>
    /// Метод для получения задачи по ее Id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="noteId"></param>
    /// <returns></returns>
    public async Task<NoteReadDto> GetNoteByIdAsync(HttpContext context, Guid noteId)
    {
        _logger.LogInformation("Попытка получения задачи по ее id");

        //получаем задачу по ее id
        var noteRead = await _noteRepository.Read(noteId);

        if (noteRead == null)
        {
            _logger.LogError("При попытке получения задачи по ее id, был передан некорректный Id");
            throw new BadHttpRequestException("При попытке получения задачи по ее id, был передан некорректный Id");
        }

        // проверяем есть ли доступ у авторизованного пользователя к этой задаче
        if (noteRead.UserId.ToString() != context.User.Identity.Name)
        {
            _logger.LogError(
                "Попытка получения доступа к задаче, к которой у авторизованного пользователя отсутствует доступ");
            throw new UnauthorizedAccessException(
                "Попытка получения доступа к задаче, к которой у авторизованного пользователя отсутствует доступ");
        }

        //маппим в dto
        var noteReadDto = noteRead.ToReadDto();

        return noteReadDto;
    }

    /// <summary>
    /// Метод для удаления задачи по ее Id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="noteId"></param>
    public async Task DeleteNoteByIdAsync(HttpContext context, Guid noteId)
    {
        _logger.LogInformation("Попытка удаления задачт по ее id");
       
        var dto = await GetNoteByIdAsync(context, noteId);
        var note = new Note
        {
            Id = dto.Id,
            BodyOfNote = dto.BodyOfNote,
            DateOfDeadLine = dto.DateOfDeadLine,
            HeadOfNote = dto.HeadOfNote,
            Images = dto.Images,
            Status = dto.Status
        };
          
        //удаляем задачу с бд
        bool deleted = await _noteRepository.Delete(note);

        //проверяем удалилась ли
        if (!deleted)
        {
            _logger.LogError("Не удалось удалить задачу с бд");
            throw new BadHttpRequestException("Не удалось удалить задачу с бд");
        }
    }

    /// <summary>
    /// Метод для обновления задачи по ее Id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="noteDto"></param>
    /// <param name="noteId"></param>
    public async Task UpdateNoteAsync(HttpContext context, NoteUpdateDto noteDto, Guid noteId)
    {
        _logger.LogInformation("Пробуем обновить задачу по ее Id");
        
        // получаем авторизованного пользователя и задачу и проверяем доступ и null
        var dto = await GetNoteByIdAsync(context, noteId);
        var note = new Note
        {
            Id = dto.Id,
            BodyOfNote = dto.BodyOfNote,
            DateOfDeadLine = dto.DateOfDeadLine,
            HeadOfNote = dto.HeadOfNote,
            Images = dto.Images,
            Status = dto.Status
        };

        //обновление изображений в задаче
        var images = new List<string>();
        var savePath = Path.Combine(_environment.WebRootPath, $@"photo\notes\{note.Id}");

        if (noteDto.Images is { Count: > 0 })
        {
            foreach (var upload in noteDto.Images)
            {
                var fullPath = await _imageService.SaveImageAsync(upload, savePath, null);

                var relativePath = Path.GetRelativePath(_environment.WebRootPath, fullPath)
                    .Replace("\\", "/");

                if (!relativePath.StartsWith("/"))
                    relativePath = "/" + relativePath;

                images.Add(relativePath);

            }
        }

        // Собираем имена новых файлов (чтобы не удалять их случайно)
        var newFileNames = noteDto.Images?.Select(u => u.FileName).ToHashSet() ?? new HashSet<string>();
        var existingImages = noteDto.ExistingImages?.ToHashSet() ?? new HashSet<string>();

        foreach (var img in note.Images.ToList()) // копия, чтобы не менять коллекцию во время итерации
        {
            bool shouldKeep = existingImages.Contains(img) || newFileNames.Contains(img);
            if (!shouldKeep)
                await _imageService.DeleteImageAsync(img, _environment.WebRootPath);
        }

        note.ApplyUpdate(noteDto, images);
        await _noteRepository.Update(note);
    }

    /// <summary>
    /// Метод для создания новой задачи
    /// </summary>
    /// <param name="context"></param>
    /// <param name="note"></param>
    public async Task CreateNoteAsync(HttpContext context, NoteCreateDto noteCreateDto)
    {
        _logger.LogInformation("Пробуем создать новую задачу");
        
        // получение id авторизованного пользователя
        var userId = Guid.Parse(context.User.Identity.Name);
            
        //маппим dto в domain
        var note = noteCreateDto.ToDomain(userId);
            
        //маппим все изображения и сохраняем их
        var images = new List<string>();
        var savePath = Path.Combine(_environment.WebRootPath, $"photo/notes/{note.Id}");

        if (noteCreateDto.Images != null)
        {
            noteCreateDto.Images.ForEach(async u =>
            {
                var relativePath = await _imageService.SaveImageAsync(u, savePath, null);
                images.Add(relativePath);
            });
        }
        note.Images = images;
            
        //сохраняем новую задачу в бд
        await _noteRepository.Create(note);
    }

}