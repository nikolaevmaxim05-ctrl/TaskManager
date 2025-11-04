namespace TaskManager.Domain.ValueObjects;

public class FriendList
{
    public List<Guid> Friends { get; set; } = new List<Guid>();
    public List<Guid> BlockedUsers { get; set; } = new List<Guid>();
    public List<Guid> ConsiderationAppl { get; set; } = new List<Guid>();
}