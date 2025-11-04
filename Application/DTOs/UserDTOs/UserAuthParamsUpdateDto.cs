namespace TaskManager.Application.DTOs.UserDTOs;

public class UserAuthParamsUpdateDto
{
    public string? EMail { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
}