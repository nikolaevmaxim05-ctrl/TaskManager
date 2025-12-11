using Moq;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.DB.Repository;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class NotificationServiceTest
{
    private readonly Mock<ILogger<NotificationService>> _logger;
    private readonly NotificationService _service;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<INotificationRepository> _notificationRepository;
    private readonly Mock<INotificationHub> _notificationHubContext;

    public NotificationServiceTest()
    {
        _logger = new Mock<ILogger<NotificationService>>();
        _userRepository = new Mock<IUserRepository>();
        _notificationRepository = new Mock<INotificationRepository>();
        _notificationHubContext = new Mock<INotificationHub>();
        _service = new NotificationService(_userRepository.Object,  _notificationRepository.Object, _logger.Object, 
            _notificationHubContext.Object);
    }

    [Fact]
    public async Task SendNotification_ReturnTrue_WhenCredentialsAreCorrect()
    {
        var notification = new Notification();
        var correctRecipientId = Guid.NewGuid();

        var recipient = new User
        {
            Id = correctRecipientId
        };
        
        _userRepository.Setup(repo => repo.GetUserByID(correctRecipientId)).ReturnsAsync(recipient);
        
        var result = await _service.SendNotificationAsync(notification, correctRecipientId);
        
        _notificationRepository.Verify(repo => repo.Create(It.IsAny<Notification>()), Times.Once);
        Assert.Contains(notification, recipient.Notifications);

        Assert.True(result);
    }
    [Fact]
    public async Task SendNotification_ReturnFalse_WhenCredentialsAreNotCorrect()
    {
        var notification = new Notification();
        var correctRecipientId = Guid.NewGuid();
        
        _userRepository.Setup(repo => repo.GetUserByID(correctRecipientId)).ReturnsAsync((User?)null);
        
        var result = await _service.SendNotificationAsync(notification, correctRecipientId);
        
        _notificationRepository.Verify(repo => repo.Create(It.IsAny<Notification>()), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteNotification_DeletingNotification_WhenCredentialsAreCorrect()
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
        };
        var notification = new Notification()
        {
            Id = Guid.NewGuid(),
        };
        
        user.Notifications.Add(notification);
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());

        await _service.DeleteNotificationAsync(context.Object, notification.Id);
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Once);
    }
    [Fact]
    public async Task DeleteNotification_BadHttpRequestException_WhenIdAreNotCorrect()
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
        };
        var notification = new Notification()
        {
            Id = Guid.NewGuid(),
        };
        
        user.Notifications.Add(notification);
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.DeleteNotificationAsync(
            context.Object, Guid.NewGuid()));
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Never);
    }
    [Fact]
    public async Task DeleteNotification_UnauthorizeAccessException_WhenIdAreNotValid()
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
        };
        var notification = new Notification()
        {
            Id = Guid.NewGuid(),
        };
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _notificationRepository.Setup(repo => repo.Read(notification.Id)).ReturnsAsync(notification);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.DeleteNotificationAsync(
            context.Object, notification.Id));
        
        _notificationRepository.Verify(repo => repo.Delete(notification), Times.Never);
    }
}