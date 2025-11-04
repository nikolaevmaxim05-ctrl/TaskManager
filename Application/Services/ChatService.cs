using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    public ChatService(ILogger<ChatService> logger,  IChatRepository chatRepository, IUserRepository userRepository, 
        IMessageRepository messageRepository)
    {
        _logger = logger;
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }
    /// <summary>
    /// Метод для получения всех чатов авторизованного пользователя
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<List<ChatReadDto>> GetAllMyChatsAsync(HttpContext context)
    {
        _logger.LogInformation("Попытка получить все чаты авторизованного пользователя");
        
        //получение Id авторизованного пользователя
        var userId = Guid.Parse(context.User.Identity.Name);

        //поиск всех чатов в которых участвует пользователь
        var chatsDto = new List<ChatReadDto>();
        (await _chatRepository.ReadAllByUserId(userId))
            .ForEach(chat => chatsDto.Add(chat.ToReadDto()));
        
        return chatsDto;
    }

    /// <summary>
    /// Метод для получения конкретного чата по его id
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ChatReadDto> GetChatByIdAsync(HttpContext context, Guid id)
    {
        _logger.LogInformation("Попытка получения конкретного чата по его Id");
        
        // считываем чат с бд и проверяем есть ли авторизованный пользователь среди его мемберов
        var chat = await ReadChatAndCheckAuth(context, id);

        return chat.ToReadDto();
    }

    /// <summary>
    /// Метод отправки сообщения в чат
    /// </summary>
    /// <param name="context"></param>
    /// <param name="chatId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SendMessage(HttpContext context, Guid chatId, MessageCreateDto message)
    {
        _logger.LogInformation("Пробуем отправить сообщение в чат");
        
        // считываем чат с бд и проверяем есть ли авторизованный пользователь среди его мемберов 
        var chat = await ReadChatAndCheckAuth(context, chatId);
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));

        //сохраняем сообщение в базе данных
        await _messageRepository.Create(message.ToDomain(user, chat));
        await _chatRepository.Update(chat);
    }

    /// <summary>
    /// Метод редактирования сообщения в чате
    /// </summary>
    /// <param name="context"></param>
    /// <param name="chatId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task EditMessage(HttpContext context, Guid chatId, MessageUpdateDto dto)
    {
        _logger.LogInformation("Пробуем редактировать сообщение в чате");
        
        // считываем чат с бд и проверяем есть ли авторизованный пользователь среди его мемберов 
        var chat = await ReadChatAndCheckAuth(context, chatId);

        // поиск редактируемого сообщения в бд по его id, проверка на null, и его редактирование
        var message = chat.Messages.Find(mes=> mes.Id == dto.Id);
        if (message == null)
        {
            _logger.LogError("Не удалось найти в бд редактируемое сообщение");
            throw new BadHttpRequestException("Не удалось найти в бд редактируемое сообщение");
        }
        message.ApplyUpdate(dto);
            
        //сохранение изменений в бд
        await _messageRepository.Update(message);
        await _chatRepository.Update(chat);
    }

    /// <summary>
    /// Метод для удаления сообщения в чате
    /// </summary>
    /// <param name="context"></param>
    /// <param name="chatId"></param>
    /// <param name="messageId"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task DeleteMessage(HttpContext context, Guid chatId, Guid messageId)
    {
        _logger.LogError("Пробуем удалить сообщение из чата");
        
        // считываем чат с бд и проверяем есть ли авторизованный пользователь среди его мемберов 
        var chat = await ReadChatAndCheckAuth(context, chatId);
        
        // поиск удаляемого сообщения в бд по его id, проверка на null, и его удаление
        var removedMessage = chat.Messages.Find(mes => mes.Id == messageId);
        if (removedMessage == null)
        {
            _logger.LogError("Не удалось найти в бд удаляемое сообщение");
            throw new BadHttpRequestException("Не удалось найти в бд удаляемое сообщение");
        }
        chat.Messages.Remove(removedMessage);

        //сохранение изменений в бд
        await _messageRepository.Delete(removedMessage);
        await _chatRepository.Update(chat);
    }

    
    /// <summary>
    /// Метод, который считывает чат по его id с бд и проверяет есть ли авторизованный пользователь среди его мемберов
    /// </summary>
    /// <param name="context"></param>
    /// <param name="chatId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    private async Task<Chat> ReadChatAndCheckAuth(HttpContext context, Guid chatId)
    {
        var user = await _userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
        var chat = await _chatRepository.ReadByChatId(chatId);

        if (chat == null)
        {
            _logger.LogError("Чат с таким id не был найден {chatId}", chatId);
            throw new BadHttpRequestException($"Чат с таким id не был найден {chatId}");
        }
        if (!chat.Members.Contains(user))
        {
            _logger.LogError("Попытка получить доступ к чату, к которому, у авторизованного пользователя нету доступа");
            throw new UnauthorizedAccessException();
        }
        
        return chat;
    }
}