using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid SenderId { get; set; }
    public string? Body { get; set; }
    public NotificationStatus  Status { get; set; }
    public NotificationType NotificationType { get; set; }
    public Notification(Guid senderId, string body, NotificationStatus status, NotificationType notificationType)
    {
        SenderId = senderId;
        Body = body;
        Status = status;
        NotificationType = notificationType;
    }

    public Notification()
    {
        
    }
}