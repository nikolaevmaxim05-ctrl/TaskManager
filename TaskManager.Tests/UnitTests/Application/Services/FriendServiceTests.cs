using Moq;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class FriendServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IChatRepository> _chatRepository;
    private readonly Mock<INotificationRepository> _notificationRepository;
    private readonly Mock<ILogger<FriendService>> _logger;
    private readonly Mock<INotificationService> _notificationService;
    private readonly IFriendServise _service;

    public FriendServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _chatRepository = new Mock<IChatRepository>();
        _notificationRepository = new Mock<INotificationRepository>();
        _logger = new Mock<ILogger<FriendService>>();
        _notificationService = new Mock<INotificationService>();
        _service = new FriendService(_userRepository.Object, _chatRepository.Object, _logger.Object, 
            _notificationService.Object, _notificationRepository.Object);
    }

    [Fact]
    public async Task GetAllMyFriends_ListFriend_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        user.FriendList.Friends.Add(friend.Id);
        friend.FriendList.Friends.Add(user.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        
        _chatRepository.Setup(repo => repo.ReadAllByUserId(user.Id)).ReturnsAsync(new List<Chat>());
        
        var result = await _service.GetAllMyFriends(context.Object);
        
        _chatRepository.Verify(repo => repo.Create(It.IsAny<Chat>()), Times.Once);
        
        Assert.Equal(1, result.Count);
        Assert.Equal(friend.Id, result[0].Id);
    } 
    
    [Fact]
    public async Task RemoveFriend_RemoveFriend_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        
        user.FriendList.Friends.Add(friend.Id);
        friend.FriendList.Friends.Add(user.Id);
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        await _service.RemoveFriend(context.Object, friend.Id);
        
        _userRepository.Verify(repo => repo.Update(friend), Times.Once);
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
    }
    [Fact]
    public async Task RemoveFriend_BadHttpException_WhenIdIsEmpty()
    {
        var user = new User{Id = Guid.NewGuid()};
        var friend = new User{Id = Guid.NewGuid()};
        
        user.FriendList.Friends.Add(friend.Id);
        friend.FriendList.Friends.Add(user.Id);
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.RemoveFriend(context.Object, friend.Id));
        
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
    }
    [Fact]
    public async Task RemoveFriend_BadHttpException_WhenFriendNotSet()
    {
        var user = new User{Id = Guid.NewGuid()};
        var friend = new User{Id = Guid.NewGuid()};
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.RemoveFriend(context.Object, friend.Id));
        
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
    }

    [Fact]
    public async Task SendFriendRequest_SendFriendRequestToUser_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        _userRepository.Setup(repo => repo.Update(It.IsAny<User>())).ReturnsAsync(true);

        await _service.SendFriendRequestAsync(context.Object, friend.Id);
        
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
        _notificationService.Verify(service => service.SendFriendRequestAsync(user.Id, friend.Id),
            Times.Once);
    }
    [Fact]
    public async Task SendFriendRequest_BadHttpException_WhenUserAlreadyFriend()
    {
        var user = new User{Id = Guid.NewGuid()};
        var friend = new User{Id = Guid.NewGuid()};
        
        user.FriendList.Friends.Add(friend.Id);
        friend.FriendList.Friends.Add(user.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.SendFriendRequestAsync(
            context.Object, friend.Id));
        
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        _notificationService.Verify(service => service.SendFriendRequestAsync(user.Id, friend.Id),
            Times.Never);
    }
    
    [Fact]
    public async Task BlockUser_UserAreBlocked_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var blockedUser = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(blockedUser.Id)).ReturnsAsync(blockedUser);
        _userRepository.Setup(repo => repo.Update(It.IsAny<User>())).ReturnsAsync(true);

        await _service.BlockUser(context.Object, blockedUser.Id);
        
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
    }
    [Fact]
    public async Task BlockUser_BadHttpRequest_WhenUserAlreadyBlocked()
    {
        var user = new User{Id = Guid.NewGuid()};
        var blockedUser = new User{Id = Guid.NewGuid()};
        
        user.FriendList.BlockedUsers.Add(blockedUser.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(blockedUser.Id)).ReturnsAsync(blockedUser);

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.BlockUser(
            context.Object, blockedUser.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
    }
    
    [Fact]
    public async Task UnBlockUser_UserAreUnBlocked_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var blockedUser = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        
        user.FriendList.BlockedUsers.Add(blockedUser.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(blockedUser.Id)).ReturnsAsync(blockedUser);
        _userRepository.Setup(repo => repo.Update(It.IsAny<User>())).ReturnsAsync(true);

        await _service.UnblockUser(context.Object, blockedUser.Id);
        
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
    }
    [Fact]
    public async Task UnBlockUser_BadHttpRequest_WhenUserNotBlocked()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var blockedUser = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(blockedUser.Id)).ReturnsAsync(blockedUser);
        _userRepository.Setup(repo => repo.Update(It.IsAny<User>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.UnblockUser(
            context.Object, blockedUser.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
    }
    
    [Fact]
    public async Task CanselFriendRequest_RequestAreCansel_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var user2 = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };
        
        user.FriendList.ConsiderationAppl.Add(user2.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(user2.Id)).ReturnsAsync(user2);
        _userRepository.Setup(repo => repo.Update(It.IsAny<User>())).ReturnsAsync(true);

        await _service.CanselFriendApplication(context.Object, user2.Id);
        
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
    }
    [Fact]
    public async Task CanselFriendRequest_BadHttpRequest_ApplicationIsNotExist()
    {
        var user = new User{Id = Guid.NewGuid()};
        var user2 = new User{Id = Guid.NewGuid()};
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(user2.Id)).ReturnsAsync(user2);

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.CanselFriendApplication(
            context.Object, user2.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
    }
    
    [Fact]
    public async Task AcceptFriendRequest_RequestAreAccepted_WhenCredentialsAreValid()  
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            SenderId = friend.Id,
        };
        
        user.Notifications.Add(notification);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        _userRepository.Setup(repo => repo.Update(It.IsAny<User>())).ReturnsAsync(true);
        
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);

        await _service.AcceptFriendRequest(context.Object, notification.Id);
        
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
        _userRepository.Verify(repo => repo.Update(friend), Times.Once);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Once);
        
        _notificationService.Verify(serv => serv.AcceptFriendRequestAsync(
            user.Id, friend.Id), Times.Once);
    }
    [Fact]
    public async Task AcceptFriendRequest_BadHttpRequest_WhenNotificationIsNull()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            SenderId = friend.Id,
        };
        
        user.Notifications.Add(notification);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
     
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.AcceptFriendRequest(
            context.Object, notification.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Never);
        
        _notificationService.Verify(serv => serv.AcceptFriendRequestAsync(
            friend.Id, user.Id), Times.Never);
    }
    [Fact]
    public async Task AcceptFriendRequest_UnauthorizedAccessException_WhenCredentialsAreNotValid()
    {
        var user = new User{Id = Guid.NewGuid()};
        var friend = new User{Id = Guid.NewGuid()};

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            SenderId = friend.Id,
        };
        
        friend.FriendList.ConsiderationAppl.Add(friend.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.AcceptFriendRequest(
            context.Object, notification.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Never);
        
        _notificationService.Verify(serv => serv.AcceptFriendRequestAsync(
            friend.Id, user.Id), Times.Never);
    }
    
    [Fact]
    public async Task DismissFriendRequest_RequestAreDismissed_WhenCredentialsAreValid()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            SenderId = friend.Id,
        };
        
        user.Notifications.Add(notification);
        
        friend.FriendList.ConsiderationAppl.Add(friend.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);

        await _service.DismissFriendRequest(context.Object, notification.Id);
        
        _userRepository.Verify(repo => repo.Update(user), Times.Once);
        _userRepository.Verify(repo => repo.Update(friend), Times.Once);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Once);
        
        _notificationService.Verify(serv => serv.DismissFriendRequestAsync(
            user.Id, friend.Id), Times.Once);
    }
    [Fact]
    public async Task DismissFriendRequest_BadHttpRequest_WhenNotificationIsNull()
    {
        var user = new User{
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "1234"},
            UserStats = new UserStats("1234", "1234")
        };
        var friend = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"},
            UserStats = new UserStats("123", "123")
        };


        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            SenderId = friend.Id,
        };
        
        friend.FriendList.ConsiderationAppl.Add(friend.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.DismissFriendRequest(
            context.Object, notification.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Never);
        
        _notificationService.Verify(serv => serv.DismissFriendRequestAsync(
            friend.Id, user.Id), Times.Never);
    }
    [Fact]
    public async Task DismissFriendRequest_UnauthorizedAccessException_WhenCredentialsAreNotValid()
    {
        var user = new User{Id = Guid.NewGuid()};
        var friend = new User{Id = Guid.NewGuid()};

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            SenderId = friend.Id,
        };
        
        friend.FriendList.ConsiderationAppl.Add(friend.Id);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(repo => repo.GetUserByID(friend.Id)).ReturnsAsync(friend);
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.DismissFriendRequest(
            context.Object, notification.Id));
        
        _userRepository.Verify(repo => repo.Update(user), Times.Never);
        _userRepository.Verify(repo => repo.Update(friend), Times.Never);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Never);
        
        _notificationService.Verify(serv => serv.DismissFriendRequestAsync(
            friend.Id, user.Id), Times.Never);
    }
}