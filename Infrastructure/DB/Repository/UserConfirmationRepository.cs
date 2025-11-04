using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB.Repository;

public class UserConfirmationRepository : IUserConfirmationRepository
{
    private readonly UserContext _context;
    private readonly ILogger<UserConfirmationRepository> _logger;

    public UserConfirmationRepository(UserContext context, ILogger<UserConfirmationRepository> logger) 
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Create(UserConfirmation entity)
    {
        _logger.LogInformation("Попытка сохранения кода подтверждения пользователя {entity.Id}", entity.Id);
        
        await _context.UserConfirmations.AddAsync(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserConfirmation?> Read(Guid id)
    {
        _logger.LogInformation($"Попытка считать код подтверждения пользователя {id} с бд");
        
        return await _context.UserConfirmations.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<UserConfirmation>> ReadAll()
    {
        _logger.LogInformation("Попытка считать все коды потверждения пользователей с  бд");
        
        return await _context.UserConfirmations.ToListAsync();
    }

    public async Task<bool> Update(UserConfirmation entity)
    {
        _logger.LogInformation("Попытка обновления кода подтверждения пользователя {entity.Id}", entity.Id);
        
        _context.UserConfirmations.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(UserConfirmation entity)
    {
        _logger.LogInformation("Попытка удаления кода подтверждения пользователя {entity.Id}", entity.Id);
        
        _context.UserConfirmations.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserConfirmation?> ReadByEmail(string email)
    {
        _logger.LogInformation($"Попытка считывания кода подтверждения пользователя по почте пользователя {email}");
        
        return await _context.UserConfirmations.FirstOrDefaultAsync(U => U.Email == email);
    }
}