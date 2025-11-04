using TaskManager.Application.DTOs.UserDTOs;

namespace TaskManager.Application.Interfaces;

public interface IFriendServise
{
    public Task<List<UserFriendReadDto>> GetAllMyFriends (HttpContext context);
    public Task RemoveFriend(HttpContext context, Guid friendId);
    public Task SendFriendRequestAsync(HttpContext context, Guid friendId);
    public Task BlockUser(HttpContext context, Guid userId);
    public Task UnblockUser(HttpContext context, Guid userId);
    public Task CanselFriendApplication(HttpContext context, Guid userId);
    public Task AcceptFriendRequest(HttpContext context, Guid notificationId);
    public Task DismissFriendRequest(HttpContext context, Guid notificationId);
}