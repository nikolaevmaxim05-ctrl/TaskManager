using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IRepository <T> where T : BaseEntity
{
    abstract public Task<bool> Create(T entity);
    abstract public Task<T?> Read(Guid id);
    abstract public Task<IEnumerable<T>> ReadAll();
    abstract public Task<bool> Update(T entity);
    abstract public Task<bool> Delete(T entity);
}