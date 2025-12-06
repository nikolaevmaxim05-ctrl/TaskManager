using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface INotificationHub
{
    public Task SendNotification(Notification notification, Guid recipientId);
}