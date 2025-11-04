using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB.Repository;

public class ChatRepository : IChatRepository
{
    private readonly UserContext _context;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ChatRepository> _logger;

    public ChatRepository(UserContext context,IUserRepository userRepository, ILogger<ChatRepository> logger)
    {
        _context = context;
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task<bool> Create(Chat entity)
    {
        _logger.LogInformation("Попытка создания чата {entity.Id}", entity.Id);
        
        await _context.Chats.AddAsync(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Chat?> Read(Guid id)
    {
        _logger.LogInformation("Попытка считать с бд чат по id {id}", id);
        
        return await _context.Chats.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Chat>> ReadAll()
    {
        _logger.LogInformation("Попытка считать все чаты с бд");
        
        return await _context.Chats.ToListAsync();
    }

    public async Task<bool> Update(Chat updatedChat)
    {
        _logger.LogInformation("Попытка обновить чат {updatedChat}", updatedChat.Id);
        
        var existingChat = await _context.Chats
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == updatedChat.Id);

        if (existingChat == null)
        {
            _logger.LogError("При попытке обновления чата, на вход репозиторию поступил null объект");
            return false;
        }

        // Обновляем участников (создаём копию, чтобы не потерять ссылки)
        var newMembers = updatedChat.Members.Select(m => m.Id).ToList();

        existingChat.Members.Clear();
        foreach (var memberId in newMembers)
        {
            var trackedUser = await _context.Users.FindAsync(memberId);
            if (trackedUser != null)
                existingChat.Members.Add(trackedUser);
        }

        // Сохраняем изменения
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(Chat entity)
    {
        _logger.LogInformation("Попытка удалить чат {entity}", entity.Id);
        
        _context.Chats.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Chat>> ReadAllByUserId(Guid id)
    {
        _logger.LogInformation($"Попытка считать все чаты пользователя {id}");
        
        var user = await _userRepository.Read(id);
        var chats = new List<Chat>();

        await _context.Chats.ForEachAsync(chat =>
        {
            if (chat.Members.Contains(user))
                chats.Add(chat);
        });

        return chats;
    }

    public async Task<Chat> ReadByChatId(Guid id)
    {
        return await _context.Chats.
            Include(c => c.Members).
            Include(c => c.Messages).
            FirstOrDefaultAsync(chat => chat.Id == id);
    }
}