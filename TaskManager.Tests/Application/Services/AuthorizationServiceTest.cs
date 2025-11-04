using System.Data;
using Moq;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class AuthorizationServiceTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<AuthorizationService>> _loggerMock;
    private readonly Mock<IUserConfirmationRepository> _userConfirmationRepoMock;
    private readonly AuthorizationService _service;

    public AuthorizationServiceTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<AuthorizationService>>();
        _userConfirmationRepoMock = new Mock<IUserConfirmationRepository>();

        _service = new AuthorizationService(
            _userRepoMock.Object,
            _loggerMock.Object,
            _userConfirmationRepoMock.Object
        );
    }
    
    [Fact]
    public async Task LogIn_ReturnsUser_WhenCredentialsAreCorrect()
    {
        var loginDto = new UserCreateDto
        {
            EMail = "test@example.com",
            Password = "CorrectPassword123"
        };

        var user = new User
        {
            AuthorizationParams = new AuthorizationParams(loginDto.EMail, BCrypt.Net.BCrypt.HashPassword(loginDto.Password))
        };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmail(loginDto.EMail)).ReturnsAsync(user);
        
        var result = await _service.LogIn(loginDto);
        
        Assert.NotNull(result);
        Assert.Equal(loginDto.EMail, result.AuthorizationParams.EMail);
    }
    [Fact]
    public async Task LogIn_ReturnsNull_WhenCredentialsAreNotCorrect()
    {
        var loginDto = new UserCreateDto
        {
            EMail = "notCorrect@gmail.com",
            Password = "notCorrectPassword123"
        };

        _userRepoMock.Setup(repo => repo.GetUserByEmail(loginDto.EMail)).ReturnsAsync((User?)null);
        
        var result = await _service.LogIn(loginDto);
        
        Assert.Null(result);
    }
    [Fact]
    public async Task LogIn_ReturnNull_WhenPasswordAreNotCorrect()
    {
        var loginDto = new UserCreateDto
        {
            EMail = "ExistMail@gmail.com",
            Password = "NotCorrectPassword123"
        };

        var user = new User
        {
            AuthorizationParams =
                new AuthorizationParams(loginDto.EMail, BCrypt.Net.BCrypt.HashPassword("correctPassword123"))
        };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmail(loginDto.EMail)).ReturnsAsync(user);
        
        var result = await _service.LogIn(loginDto);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task SignIn_ReturnUser_WhenCredentialsAreCorrect()
    {
        var signInDto = new UserCreateDto
        {
            EMail = "test@example.com",
            Password = "CorrectPassword123",
            Code = "CorrectToken"
        };

        var confir = new UserConfirmation
        {
            Email = "test@example.com",
            Id = Guid.NewGuid(),
            Token = "CorrectToken"
        };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmail(signInDto.EMail)).ReturnsAsync((User?)null);
        _userConfirmationRepoMock.Setup(repo => repo.ReadByEmail(signInDto.EMail)).ReturnsAsync(confir);
        
        var result = await _service.SignIn(signInDto);
        
        Assert.NotNull(result);
        Assert.Equal(signInDto.EMail, result.AuthorizationParams.EMail);
        
        _userRepoMock.Verify(repo => repo.Create(It.IsAny<User>()), Times.Once);
        _userConfirmationRepoMock.Verify(repo => repo.Delete(It.IsAny<UserConfirmation>()), Times.Once);

        // Проверяем, что пароль был захеширован
        Assert.True(BCrypt.Net.BCrypt.Verify(signInDto.Password, result.AuthorizationParams.PasswordHash));
    }
    [Fact]
    public async Task SignIn_ThrowsDuplicateNameException_WhenUserAlreadyExist()
    {
        var signInDto = new UserCreateDto
        {
            EMail = "Correct@gmail.com",
            Password = "CorrectPassword123",
            Code = "123"
        };

        var confirm = new UserConfirmation
        {
            Email = "Correct@gmail.com",
            Token = "123"
        };

        var user = new User
        {
            AuthorizationParams =
                new AuthorizationParams(signInDto.EMail, BCrypt.Net.BCrypt.HashPassword(signInDto.Password))
        };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmail(signInDto.EMail)).ReturnsAsync(user);
        _userConfirmationRepoMock.Setup(repo => repo.ReadByEmail(signInDto.EMail)).
            ReturnsAsync(confirm);
        
        await Assert.ThrowsAsync<DuplicateNameException>(() => _service.SignIn(signInDto));
        _userRepoMock.Verify(repo => repo.Create(It.IsAny<User>()), Times.Never);
        
    }
    [Fact]
    public async Task SignIn_BadHttpException_WhenTokenAreInvalid()
    {
        var signInDto = new UserCreateDto
        {
            EMail = "Correct@gmail.com",
            Password = "CorrectPassword123",
            Code = "123"
        };

        var confirm = new UserConfirmation
        {
            Email = "Correct@gmail.com",
            Token = "tiInvalid"
        };

        var user = new User
        {
            AuthorizationParams =
                new AuthorizationParams(signInDto.EMail, BCrypt.Net.BCrypt.HashPassword(signInDto.Password))
        };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmail(signInDto.EMail)).ReturnsAsync((User?)null);
        _userConfirmationRepoMock.Setup(repo => repo.ReadByEmail(signInDto.EMail)).
            ReturnsAsync(confirm);
        
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.SignIn(signInDto));
        _userRepoMock.Verify(repo => repo.Create(It.IsAny<User>()), Times.Never);
        
    }

    [Fact]
    public async Task CodeConfirmation_ReturnTrue_WhenCredentialsAreCorrect()
    {
        var correctToken = "correctToken123";
        var correctEmail = "correct@gmail.com";

        var userConfirmation = new UserConfirmation
        {
            Email = correctEmail,
            Token = correctToken
        };
        
        _userConfirmationRepoMock.Setup(repo => repo.ReadByEmail(correctEmail))
            .ReturnsAsync(userConfirmation);
        
        var result = await _service.CodeConfirmation(correctToken, correctEmail);
        
        Assert.True(result);
    }
    [Fact]
    public async Task CodeConfirmation_ReturnFalse_WhenTokenAreNotCorrect()
    {
        var unCorrectToken = "uncorrectToken123";
        var correctEmail = "correct@gmail.com";

        var userConfirmation = new UserConfirmation
        {
            Email = correctEmail,
            Token = "CorrectToken"
        };
        
        _userConfirmationRepoMock.Setup(repo => repo.ReadByEmail(correctEmail)).ReturnsAsync(userConfirmation);
        
        var result = await _service.CodeConfirmation(unCorrectToken, correctEmail);
        
        Assert.False(result);
    }
    [Fact]
    public async Task CodeConfirmation_ReturnFalse_WhenCredentialsAreNotCorrect()
    {
        var unCorrectToken = "uncorrectToken123";
        var unCorrectEmail = "uncorrect@gmail.com";
        
        _userConfirmationRepoMock.Setup(repo => repo.ReadByEmail(unCorrectEmail))
            .ReturnsAsync((UserConfirmation?)null);
        
        var result = await _service.CodeConfirmation(unCorrectToken, unCorrectEmail);
        
        Assert.False(result);
    }
}