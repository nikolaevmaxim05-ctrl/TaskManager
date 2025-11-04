using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB.Repository;

public class MessageRepository : IMessageRepository
{
    private readonly UserContext _context;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(UserContext context, ILogger<MessageRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<bool> Create(Message entity)
    {
        _logger.LogInformation("Попытка создания сообщения {entity.Id}", entity.Id);
        
        await _context.Messages.AddAsync(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Message?> Read(Guid id)
    {
        _logger.LogInformation("Попытка считать с бд сообщение по id {id}", id);
        
        return await _context.Messages.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Message>> ReadAll()
    {
        _logger.LogInformation("Попытка считать все сообщения с бд");
        
        return await _context.Messages.ToListAsync();
    }

    public async Task<bool> Update(Message entity)
    {
        _logger.LogInformation("Попытка обновить сообщение {entity}", entity.Id);
        
        _context.Messages.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(Message entity)
    {
        _logger.LogInformation("Попытка удалить сообщение {entity}", entity.Id);
        
        _context.Messages.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}