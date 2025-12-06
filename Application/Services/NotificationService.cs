using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Services;

public class NotificationService: INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHub _notificationHubContext;

    public NotificationService(IUserRepository userRepository, INotificationRepository notificationRepository,
        ILogger<NotificationService> logger,  INotificationHub notificationHubContext)
    {
        _logger = logger;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _notificationHubContext = notificationHubContext;
    }
    public async Task<bool> SendNotificationAsync(Notification notification, Guid recipientId)
    {
        _logger.LogInformation($"Попытка отправить уведомление {notification.Id} пользователю {recipientId}, " +
                               $"отправитель {notification.SenderId}");
        
        //получаем получателя уведомления с бд по id
        var recipient = await _userRepository.GetUserByID(recipientId);

        if (recipient == null)
        {
            _logger.LogError($"Не получилось найти пользователя {recipientId} в бд, которому должно было " +
                             $"быть отправленно уведомление {notification.Id}");
            
            return false;
        }

        //добавляем получатель уведомления новое уведомление, а затем сохраняем это уведомление в базе данных
        recipient.Notifications.Add(notification);

        await _notificationRepository.Create(notification);
        
        await _notificationHubContext.SendNotification(notification, recipientId);
        
        return true;
    }

    public async Task<bool> SendFriendRequestAsync(Guid sender, Guid recipient)
    {
        string nickName = (await _userRepository.GetUserByID(sender)).UserStats.NickName;
        return await SendNotificationAsync(
            new Notification(sender, $"Пользователь {nickName} отправил вам запрос в друзья", 
                NotificationStatus.Unread, NotificationType.FriendRequest), recipient);
    }

    public async Task<bool> DismissFriendRequestAsync(Guid sender, Guid recipient)
    {
        string nickName = (await _userRepository.GetUserByID(sender)).UserStats.NickName;
        return await SendNotificationAsync(
            new Notification(sender, $"Пользователь {nickName} отклонил ваш запрос в друзья",
                NotificationStatus.Unread, NotificationType.Message), recipient);
    }

    public async Task<bool> AcceptFriendRequestAsync(Guid sender, Guid recipient)
    {
        string nickName = (await _userRepository.GetUserByID(sender)).UserStats.NickName;
        return await SendNotificationAsync(
            new Notification(sender, $"Пользователь {nickName} принял ваш запрос в друзья", 
                NotificationStatus.Unread, NotificationType.Message), recipient);
    }

    /// <summary>
    /// Метод для удаления уведомлений
    /// </summary>
    /// <param name="context"></param>
    /// <param name="notificationId"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task DeleteNotificationAsync(HttpContext context, Guid notificationId)
    {
        _logger.LogInformation("Пробуем удалить уведомление");    
        
        //получаем авторизованного пользователя и уведомление по их ID
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
        var notification = await _notificationRepository.Read(notificationId);
        
        //проверяем уведомление на null
        if (user == null || notification == null)
        {
            _logger.LogError("При попытке удаления уведомления, на вход методу поступил некорректный Id");
            throw new BadHttpRequestException(
                "При попытке удаления уведомления, на вход методу поступил некорректный Id");
        }
        
        //проверяем доступ авторизованного пользователя к этому уведомлению
        if (!user.Notifications.Contains(notification))
        {
            _logger.LogError(
                "Попытка получения доступа уведомлению, к которому, у авторизованного пользователя отсутствует доступ");
            throw new UnauthorizedAccessException();
        }

        //удаляем уведомления с бд
        await _notificationRepository.Delete(notification);
    }
}