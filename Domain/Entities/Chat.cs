using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

public class Chat : BaseEntity
{
    public List<User> Members { get; set; } = new();
    public List<Message> Messages { get; set; } = new();

    public Chat(Guid id, List<User> members, List<Message> messages)
    {
        Id = id;
        Members = members;
        Messages = messages;
    }

    public Chat() { }
}