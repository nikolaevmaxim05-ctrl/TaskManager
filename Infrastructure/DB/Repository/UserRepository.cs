using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(UserContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        private UserContext _context { get; set; }  
        public async Task<bool> Create(User entity)
        {
            _logger.LogInformation("Попытка сохранения пользователя {entity.Id}", entity.Id);
            
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(User entity)
        {
            _logger.LogInformation("Попытка удаления пользователя {entity.Id}", entity.Id);
            
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> Read(Guid id)
        {
            _logger.LogInformation("Попытка считать с бд пользователя с id {id}", id);
            
            return await _context.Users
                .Include(u => u.NotePool)
                .Include(u => u.Notifications)
                .Include(u=>u.Chats)
                .FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task<bool> Update(User entity)
        {
            _logger.LogInformation("Попытка обновить пользователя {entity.Id}", entity.Id);
            
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> ReadAll()
        {
            _logger.LogInformation("Попытка считать с бд всех пользователей");
            
            return await _context.Users
                .Include(u => u.NotePool)
                .Include(u => u.Notifications)
                .Include(u=>u.Chats)
                .ToListAsync();
        }

        
        public async Task<User?> GetUserByID(Guid ID)
        {
            _logger.LogInformation("Попытка считать пользователя с бд по id {ID}", ID);
            
            return await _context.Users
                .Include(u => u.NotePool)
                .Include(u => u.Notifications)
                .Include(u=>u.Chats)
                .FirstOrDefaultAsync(user => user.Id == ID);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            _logger.LogInformation("Попытка считать пользователя с бд по почте {email}", email);
            
            return await _context.Users
                .Include(u => u.NotePool)
                .Include(u => u.Notifications)
                .Include(u=>u.Chats)
                .FirstOrDefaultAsync(u => u.AuthorizationParams.EMail == email);
        }
        public async Task<IEnumerable<User>> SearchUsersAsync(string query)
        {
            _logger.LogInformation($"Попытка поиска пользователя по никнэйму {query}");
            
            query = query.ToLower().Trim();

            return await _context.Users
                .Where(u =>
                    u.UserStats.NickName.ToLower().Contains(query) ||
                    u.AuthorizationParams.EMail.ToLower().Contains(query))
                .Take(20) // ограничим до 20 результатов
                .ToListAsync();
        }
    }
}
