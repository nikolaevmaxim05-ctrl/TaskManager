using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface INotificationService
{
    public Task<bool> SendNotificationAsync(Notification notificaton, Guid recipient);
    public Task<bool> SendFriendRequestAsync(Guid sender, Guid recipient);
    public Task<bool> DismissFriendRequestAsync(Guid sender, Guid recipient);
    public Task<bool> AcceptFriendRequestAsync(Guid sender, Guid recipient);
    public Task DeleteNotificationAsync(HttpContext context, Guid notificationId);
}