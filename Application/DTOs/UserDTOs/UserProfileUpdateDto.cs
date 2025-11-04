namespace TaskManager.Application.DTOs.UserDTOs;

public class UserProfileUpdateDto
{
    public string? NickName { get; set; } = string.Empty;
    public IFormFile? Avatar { get; set; }
}