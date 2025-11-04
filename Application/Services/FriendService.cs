using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class FriendService : IFriendServise
{
    private readonly IUserRepository _userRepository;
    private readonly IChatRepository _chatRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<FriendService> _logger;
    private readonly INotificationService _notificationService;
    
    public FriendService(IUserRepository userRepository, IChatRepository chatRepository, ILogger<FriendService> logger,
        INotificationService notificationService, INotificationRepository notificationRepository)
    {
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _logger = logger;
        _notificationService = notificationService;
        _notificationRepository = notificationRepository;
    }
    /// <summary>
    /// Метод для получения всех друзей авторизованного пользователя
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<List<UserFriendReadDto>> GetAllMyFriends(HttpContext context)
    {
        _logger.LogInformation("Пробуем получить всех друзей авторизованного пользователя");
        
        //получаем авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));

        // список друзей который будем возвращать
        var friends = new List<UserFriendReadDto>();
            
        //дополнительная проверка, если чат между пользователями не создан, то он автоматически создается
        var chats = await _chatRepository.ReadAllByUserId(user.Id);
            
        //проходимся по всем id из списка друзей авторизованного пользователя и добавляем их в возвращаемый список друзей
        //а так же, создаем чат, если тот между ними отсутствует
        foreach (var u in user.FriendList.Friends)
        {
            var friend = await _userRepository.GetUserByID(u);

            var chat = chats.FirstOrDefault(c =>
                c.Members.Contains(user) && c.Members.Contains(friend) && c.Members.Count == 2);

            if (chat == null)
            {
                chat = new Chat(Guid.NewGuid(), new List<User>
                {
                    user, friend
                }, new List<Message>());
                await _chatRepository.Create(chat);
            }
                
            friends.Add(friend.ToFriendReadDto(chat.ToReadDto()));
        }
        
        return friends;
    }

    /// <summary>
    /// Метод для удаления друга по его Id из списка друзей авторизованного пользователя
    /// </summary>
    /// <param name="context"></param>
    /// <param name="friendId"></param>
    public async Task RemoveFriend(HttpContext context, Guid friendId)
    {
        _logger.LogInformation("Пробуем удалить друга по его Id из списка друзей авторизованного пользователя");
        
        //получаем авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));

        //получаем друга по его Id и проверяем на null
        var deletedFriend = await _userRepository.GetUserByID(friendId);
        if (deletedFriend == null)
        {
            _logger.LogError(
                "При попытке удалить друга, на вход методу поступил id по которому не было найдено пользователей");
            throw new BadHttpRequestException(
                "При попытке удалить друга, на вход методу поступил id по которому не было найдено пользователей");
        }

        if (!user.FriendList.Friends.Contains(friendId))
        {
            _logger.LogError("Попытка удалить из друзей пользователя которого и так не было в друзьях");
            throw new BadHttpRequestException(
                "Попытка удалить из друзей пользователя которого и так не было в друзьях");
        }
         
        //удаляем из списков друзей id пользователей
        user.FriendList.Friends.Remove(friendId);
        deletedFriend.FriendList.Friends.Remove(user.Id);

        // сохраняем изменения в бд
        await _userRepository.Update(user);
        await _userRepository.Update(deletedFriend);
    }
    
    /// <summary>
    /// Метод для отправки запросов на дружбу пользователю по его id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="friendId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SendFriendRequestAsync(HttpContext context, Guid friendId)
    {
        _logger.LogInformation("Пробуем отправить заявку на дружбу пользователь по его Id");
        
        //получаем авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
        
        //проверяем нет ли этого пользователя в друзьях или не была отправлена заявка ранее
        if (user.FriendList.ConsiderationAppl.Contains(friendId) || user.FriendList.Friends.Contains(friendId))
        {
            _logger.LogError("Невозможно отправить заявку на дружбу, так как пользователь уже является другом");
            throw new BadHttpRequestException("Невозможно отправить заявку на дружбу, так как пользователь уже является другом");
        }
            
        // добавляем пользователя в список ожидающих заявок
        user.FriendList.ConsiderationAppl.Add(friendId);

        //обновляем пользователя в бд
        if (!await _userRepository.Update(user))
        {
            _logger.LogError("Не удалось обновить пользователя в бд");
            throw new BadHttpRequestException("Не удалось обновить пользователя в бд");
        }

        //отправляем уведомления об заявке на дружбу пользователю
        await _notificationService.SendFriendRequestAsync(user.Id, friendId);
    }

    /// <summary>
    /// Метод для блокировки пользователя по его id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userId"></param>
    public async Task BlockUser(HttpContext context, Guid userId)
    {
        _logger.LogInformation("Пробуем заблокировать пользователя по его id");
        
        //получаем авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));

        // проверяем не блокировли ли этого пользователя ранее
        if (user.FriendList.BlockedUsers.Contains(userId))
        {
            _logger.LogError("Попытка заблокировать пользователя который и так уже заблокирован");
            throw new BadHttpRequestException("Попытка заблокировать пользователя который и так уже заблокирован");
        }
            
        //добавляем пользователя в список заблокированных
        user.FriendList.BlockedUsers.Add(userId);
            
        //обновляем пользователя в бл
        if (!await _userRepository.Update(user))
        {
            _logger.LogError("Не удалось обновить пользователя в бд");
            throw new BadHttpRequestException("Не удалось обновить пользователя в бд");
        }
    }

    /// <summary>
    /// Метод для разблокировки пользователя по его id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userId"></param>
    public async Task UnblockUser(HttpContext context, Guid userId)
    {
        _logger.LogInformation("Пробуем разблокировать пользователя по его id");
        
        //получаем аваторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));

        //проверяем находится ли пользователь в списке заблокированных
        if (!user.FriendList.BlockedUsers.Contains(userId))
        {
            _logger.LogError("Попытка разблокировать пользователя, который не был заблокирован");
            throw new BadHttpRequestException("Попытка разблокировать пользователя, который не был заблокирован");
        }
            
        //убираем пользователя из списка заблокированных
        user.FriendList.BlockedUsers.Remove(userId);
            
        // обновляем данные пользователя в бд
        if (!await _userRepository.Update(user))
        {
            _logger.LogError("Не удалось сохранить пользователя в бд");
            throw new BadHttpRequestException("Не удалось сохранить пользователя в бд");
        }
    }

    /// <summary>
    /// Метод для отмены отправленной заявки на дружбу по Id пользователя
    /// </summary>
    /// <param name="context"></param>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task CanselFriendApplication(HttpContext context, Guid userId)
    {
        _logger.LogInformation("Пробуем отменить заявку на дружбу");
        
        //получаем авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));

        //проверяем была ли заявка когда-нибудь отправлена
        if (!user.FriendList.ConsiderationAppl.Contains(userId))
        {
            _logger.LogError("Ошибка при отмене заявки на дружбу: нельзя отменить заявку которой не было");
            throw new BadHttpRequestException("Ошибка при отмене заявки на дружбу: нельзя отменить заявку которой не было");
        }
            
        //удаляем заявку
        user.FriendList.ConsiderationAppl.Remove(userId);

        //обновляем пользователя в бд
        if (!await _userRepository.Update(user))
        {
            _logger.LogError("Ошибка при обновлении пользователя в бд");
            throw new BadHttpRequestException("Ошибка при обновлении пользователя в бд");
        }
    }

    /// <summary>
    /// Метод для принятия запроса в друзья через уведомление
    /// </summary>
    /// <param name="context"></param>
    /// <param name="notificationId"></param>
    public async Task AcceptFriendRequest(HttpContext context, Guid notificationId)
    {
        // получаем авторизованного пользователя
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
            
        //получаем уведомление по id а так же отправителя уведомления
        var notification = await _notificationRepository.Read(notificationId);

        if (notification == null)
        {
            _logger.LogError("При попытке принять запрос в друзья, на вход методу поступил некорректный id");
            throw new BadHttpRequestException("При попытке принять запрос в друзья, на вход методу поступил некорректный id");
        }
        
        var sender = await _userRepository.GetUserByID(notification.SenderId);
        
        //проверяем есть ли доступ у авторизованного пользователя к данному уведомлению
        if (!user.Notifications.Contains(notification))
        {
            _logger.LogError(
                "Попытка доступа к уведомлению, к которому, у авторизованного пользователя отсутствует доступ");
            throw new UnauthorizedAccessException();
        }
            
        // добавляем друг друга в список друзей
        user.FriendList.Friends.Add(notification.SenderId);
        sender.FriendList.Friends.Add(user.Id);
         
        //удаляем заявку на дружбу
        sender.FriendList.ConsiderationAppl.Remove(user.Id);
        await _notificationRepository.Delete(notification);

        //обновляем пользователей в бд
        await _userRepository.Update(user);
        await _userRepository.Update(sender);
        
        
        // отправляем уведомление другу о том что мы приняли его запрос в друзья
        await _notificationService.AcceptFriendRequestAsync(user.Id,sender.Id);
    }

    /// <summary>
    /// Метод для отклонения запроса в друзья через уведомления
    /// </summary>
    /// <param name="context"></param>
    /// <param name="notificationId"></param>
    public async Task DismissFriendRequest(HttpContext context, Guid notificationId)
    {
        _logger.LogInformation("Пробуем отклонить запрос в друзья");
        
        //получаем авторизованного пользователя, а так же уведомления с отправителем по их ID
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
        var notification = await _notificationRepository.Read(notificationId);
        
        //проверяем уведомление на Null
        if (notification == null)
        {
            _logger.LogError("При попытке отклонить запрос в друзья, на вход методу поступил некорректный ID");
            throw new BadHttpRequestException(
                "При попытке отклонить запрос в друзья, на вход методу поступил некорректный ID");
        }
        
        var sender = await _userRepository.GetUserByID(notification.SenderId);

        // проверяем есть ли доступ у авторизованного пользователю к этому уведомлению
        if (!user.Notifications.Contains(notification))
        {
            _logger.LogError("Попытка доступа уведомлению, к которому, у авторизованного пользователя отсутствует доступ");
            throw new UnauthorizedAccessException();
        }
            
        //удаляем заявку на дружбу
        sender.FriendList.ConsiderationAppl.Remove(user.Id);
        await _notificationRepository.Delete(notification);

        //обновляем пользователей в бд
        await _userRepository.Update(user);
        await _userRepository.Update(sender);
        
        //отправляем уведомление о том, что заявка была отклонена
        await _notificationService.DismissFriendRequestAsync( user.Id, sender.Id);
    }
}