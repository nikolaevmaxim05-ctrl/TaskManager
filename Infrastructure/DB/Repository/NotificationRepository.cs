using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly UserContext _context;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(UserContext context,ILogger<NotificationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<bool> Create(Notification entity)
    {
        _logger.LogInformation("Попытка сохранения уведомления {entity.Id}", entity.Id);
        
        await _context.Notifications.AddAsync(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Notification?> Read(Guid id)
    {
        _logger.LogInformation($"Попытка считать уведомление с бд {id}");
        
        return await _context.Notifications.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Notification>> ReadAll()
    {
        _logger.LogInformation("Попытка считать все уведомления пользователей с бд");
        
        return await _context.Notifications.ToListAsync();
    }

    public async Task<bool> Update(Notification entity)
    {
        _logger.LogInformation("Попытка обновления уведомления {entity.Id}", entity.Id);
        
        _context.Notifications.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(Notification entity)
    {
        _logger.LogInformation("Попытка удаления уведомления {entity.Id}", entity.Id);
        
        _context.Notifications.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}