using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Mappers;

public static class UserMapper
{
    public static User ToDomain(this UserCreateDto dto)
        => new()
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams(dto.EMail, BCrypt.Net.BCrypt.HashPassword(dto.Password)),
            NotePool = new List<Note>(),
            UserStats = new UserStats(dto.NickName, null),
            FriendList = new FriendList(),
            Notifications = new List<Notification>()
        };

    public static UserProfileReadDto ToReadDto(this User user) 
        => new()
        {
            NickName = user.UserStats.NickName,
            AvatarPath = user.UserStats.AvatarPath,
            EMail = user.AuthorizationParams.EMail,
            FriendList = user.FriendList.Friends
        };
    public static UserSearchProfileReadDto ToSearchReadDto(this User user) 
        => new()
        {
            NickName = user.UserStats.NickName,
            AvatarPath = user.UserStats.AvatarPath,
            EMail = user.AuthorizationParams.EMail,
            Id = user.Id
        };
    
    public static UserFriendReadDto ToFriendReadDto (this User user, ChatReadDto chat)
        => new()
        {
            Id = user.Id,
            NickName = user.UserStats.NickName,
            AvatarPath = user.UserStats.AvatarPath,
            EMail = user.AuthorizationParams.EMail,
            Chat = chat
        };

    public static void ApplyUpdate(this User user, UserProfileUpdateDto updateDto)
    {
        user.UserStats.NickName = updateDto.NickName;
        if (updateDto.Avatar !=null)
            user.UserStats.AvatarPath = updateDto.Avatar.FileName;
    }
    public static void ApplyUpdate(this User user, UserAuthParamsUpdateDto authParamsUpdateDto)
    {
        user.AuthorizationParams.EMail = authParamsUpdateDto.EMail;
        user.AuthorizationParams.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authParamsUpdateDto.NewPassword);
    }
}