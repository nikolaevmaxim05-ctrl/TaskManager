using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    public Task<User?> GetUserByID(Guid ID);
    public Task<User?> GetUserByEmail(string email);
    public Task<IEnumerable<User>> SearchUsersAsync(string query);
}