using TaskManager.Application.DTOs.ChatDTOs;

namespace TaskManager.Application.DTOs.UserDTOs;

public class UserFriendReadDto
{
    public string? EMail { get; set; }
    public string? NickName { get; set; }
    public string? AvatarPath { get; set; }
    public List<Guid> FriendList { get; set; }
    public ChatReadDto Chat { get; set; }
    public Guid Id { get; set; }
}