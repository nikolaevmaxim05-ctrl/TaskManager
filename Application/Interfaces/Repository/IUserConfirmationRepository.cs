using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IUserConfirmationRepository:IRepository<UserConfirmation>
{
    public Task<UserConfirmation?> ReadByEmail(string email);
}