using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IChatHub
{
    public Task SendMessage(Message message);
    public Task UpdateMessage(Message message);
    public Task DeleteMessage(Message message);
}