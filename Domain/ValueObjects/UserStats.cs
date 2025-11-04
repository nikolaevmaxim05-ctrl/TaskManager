namespace TaskManager.Domain.ValueObjects;

public class UserStats
{
    public string? NickName { get; set; }
    public string? AvatarPath { get; set; }
    public UserStats(string nickName, string avatarPath)
    {
        NickName = nickName;
        AvatarPath = avatarPath;
    }
}