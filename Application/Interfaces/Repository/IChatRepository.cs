using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IChatRepository : IRepository<Chat>
{
    public Task<List<Chat>> ReadAllByUserId(Guid id);
    public Task<Chat> ReadByChatId(Guid id);
}