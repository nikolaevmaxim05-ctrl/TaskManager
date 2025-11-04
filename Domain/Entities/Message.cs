using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ChatId { get; set; }       // FK на Chat
    public Chat Chat { get; set; }         // Навигация обратно

    public Guid SenderId { get; set; }     // FK на User
    public User Sender { get; set; }       // Навигация

    public string Body { get; set; }
    public DateTime SendTime { get; set; }

    public Message(Guid id, User sender, string body, DateTime sendTime)
    {
        Id = id;
        Sender = sender;
        SenderId = sender.Id;
        Body = body;
        SendTime = sendTime;
    }

    public Message() { }
}