namespace TaskManager.Application.DTOs.UserDTOs;

public class UserSearchProfileReadDto
{
    public Guid Id { get; set; }
    public string? EMail { get; set; }
    public string? NickName { get; set; }
    public string? AvatarPath { get; set; }
}