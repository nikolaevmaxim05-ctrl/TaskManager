using Moq;
using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class UserConfirmationServiceTests
{
    private readonly Mock<IEmailService> _emailService;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IUserConfirmationRepository> _userConfirmRepo;
    private readonly Mock<ILogger<UserConfirmationService>> _logger;
    private readonly UserConfirmationService _service;

    public UserConfirmationServiceTests()
    {
        _emailService = new Mock<IEmailService>();
        _userRepo = new Mock<IUserRepository>();
        _userConfirmRepo = new Mock<IUserConfirmationRepository>();
        _logger = new Mock<ILogger<UserConfirmationService>>();
        _service = new UserConfirmationService(_emailService.Object, _userRepo.Object, _userConfirmRepo.Object,
            _logger.Object);
    }
    [Fact]
    public async Task SendConfirmationEmail_SendEmail_WhenCredentialsAreCorrect()
    {
        var dto = new UserConfirmationsCreateDto
        {
            EMail = "123",
        };
        
        _userRepo.Setup(repo => repo.GetUserByEmail(dto.EMail)).ReturnsAsync((User?) null);
        
        await _service.SendConfirmationEmail(dto);
        
        _userConfirmRepo.Verify(repo => repo.Create(It.IsAny<UserConfirmation>()), Times.Once);
        _emailService.Verify(service => service.SendMailAsync(It.IsAny<string>(),It.IsAny<string>(),
            It.IsAny<string>() ), Times.Once);
    }

    [Fact]
    public async Task SendConfirmationEmail_NullRefExсeption_WhenUserIsExist()
    {
        var dto = new UserConfirmationsCreateDto
        {
            EMail = "123",
        };
        
        _userRepo.Setup(repo => repo.GetUserByEmail(dto.EMail)).ReturnsAsync(new User());
        
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.SendConfirmationEmail(dto));
        
        _userRepo.Verify(repo => repo.Create(It.IsAny<User>()), Times.Never);
        _emailService.Verify(service => service.SendMailAsync(It.IsAny<string>(),It.IsAny<string>(),
            It.IsAny<string>() ), Times.Never);
    }
}