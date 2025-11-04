namespace TaskManager.Application.DTOs.UserDTOs;

public class UserProfileReadDto
{
    public string? EMail { get; set; }
    public string? NickName { get; set; }
    public string? AvatarPath { get; set; }
    public List<Guid> FriendList { get; set; }
}