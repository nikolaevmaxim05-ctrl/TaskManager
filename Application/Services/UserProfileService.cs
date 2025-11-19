using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ILogger<UserProfileService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IImageService _imageService;
    private readonly IWebHostEnvironment _env;

    public UserProfileService(ILogger<UserProfileService> logger, IUserRepository userRepository,
        IImageService imageService, IWebHostEnvironment env)
    {
        _logger =  logger;
        _userRepository = userRepository;
        _imageService = imageService;
        _env = env;
    }
    
    /// <summary>
    /// Метод для определения статуса профиля по отношению к авторизованному пользователю
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ProfileStatus> GetProfileStatusByID(HttpContext context, Guid id)
    {
        _logger.LogInformation("Пробуем определить статус профиля, по отношению к авторизованному пользователю");
        
        //получение авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
        
        //если указан id авторизованного пользователя, то возвращаем Mine
        if (id == user.Id)
            return ProfileStatus.Mine;

        // ищем пользователя в списке друзей, и возвращаем соответствующий статус, если он в нем находится
        var friends = user.FriendList;
        return 
            friends.Friends.Contains(id) ? ProfileStatus.Friend :
            friends.ConsiderationAppl.Contains(id) ? ProfileStatus.ConsiderationApply :
            friends.BlockedUsers.Contains(id) ? ProfileStatus.Banned :
            ProfileStatus.Unfamilar;
    }

    /// <summary>
    /// Метод для обновления профиля авторизованного пользователя
    /// </summary>
    /// <param name="context"></param>
    /// <param name="dto"></param>
    /// <returns></returns>0
    public async Task UpdateProfile(HttpContext context, UserProfileUpdateDto dto)
    {
        _logger.LogInformation("Пробуем обновить профиль авторизованного пользователя");
        
        // Получаем авторизованного пользователя
        var userId = Guid.Parse(context.User.Identity!.Name!);
        var user = await _userRepository.GetUserByID(userId);
        if (user == null)
        {
            _logger.LogError("Не авторизирован");
            throw new UnauthorizedAccessException();
        }

        string relativePath = null;
        if (dto.Avatar != null)
        {
            // Папка для аватарок
            var avatarDir = Path.Combine(_env.WebRootPath, "photo", "ava");

            // Сохраняем аватар
            relativePath = await _imageService.SaveImageAsync(dto.Avatar, avatarDir, user.UserStats.AvatarPath);
        }

        // Обновляем остальные поля пользователя
        user.ApplyUpdate(dto);
        user.UserStats.AvatarPath = relativePath;

        //  Сохраняем изменения
        await _userRepository.Update(user);
    }
}