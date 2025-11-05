using System.Security.Cryptography;
using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class UserConfirmationService : IUserConfirmationService
{
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepo;
    private readonly IUserConfirmationRepository _userConfirmRepo;
    private readonly ILogger<UserConfirmationService> _logger;
    

    public UserConfirmationService(IEmailService emailService, IUserRepository userRepo, 
        IUserConfirmationRepository userConfirmRepo, ILogger<UserConfirmationService> logger)
    {
        _emailService = emailService;
        _userRepo = userRepo;
        _userConfirmRepo = userConfirmRepo;
        _logger = logger;
    }
    
    /// <summary>
    /// Метод для отправки кода подтверждения на почту
    /// </summary>
    /// <param name="dto"></param>
    public async Task SendConfirmationEmail(UserConfirmationsCreateDto dto)
    {
        _logger.LogInformation("Попытка отправить код подтверждения на почту пользователя");
        
        //проверяем нет ли пользователя с такой почтой в базе данных
        if (await _userRepo.GetUserByEmail(dto.EMail) != null)
            throw new BadHttpRequestException("Пользователь с таким email уже существует");
        
        //генерация кода подтверждения
        var tokenBytes = RandomNumberGenerator.GetBytes(32); // 32 байта случайных данных
        var token = Convert.ToBase64String(tokenBytes); // Кодируем в base64
        token = token.Replace("+", "").Replace("/", "").Replace("=", "");
        
        // сохраняем в бд код подтверждения
        var userConfirmation = dto.ToDomain(token);
        await _userConfirmRepo.Create(userConfirmation);

        // отправляем код подтверждения на почту пользователю
        await _emailService.SendMailAsync(userConfirmation.Email, "Подтверждение регистрации на сайте TaskManager", token);
    }
}
