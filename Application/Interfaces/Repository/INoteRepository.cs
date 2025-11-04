using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface INoteRepository : IRepository<Note>
{
    public Task<IEnumerable<Note>> ReadAllByUserId(Guid userId);
}