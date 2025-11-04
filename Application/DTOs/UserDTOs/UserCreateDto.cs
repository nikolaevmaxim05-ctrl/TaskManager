namespace TaskManager.Application.DTOs.UserDTOs;

public class UserCreateDto 
{
    public string? EMail { get; set; }
    public string? Password { get; set; }
    public string? NickName { get; set; }
    public string? Code { get; set; }
}