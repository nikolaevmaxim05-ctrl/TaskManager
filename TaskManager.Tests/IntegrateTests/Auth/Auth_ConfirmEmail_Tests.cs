using System.Net;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.DB;
using Xunit;

namespace TaskManager.TaskManager.Tests.IntegrateTests.Auth;

public class Auth_ConfirmEmail_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public Auth_ConfirmEmail_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConfirmEmail_Success_CredentialsAreCorrect()
    {
        // Arrange
        var userDto = new UserCreateDto
        {
            Code = "correctToken1",
            EMail = "correctEmail1@mail.ru",
            Password = "Abcabc123_123_"
        };
        var confCode = new UserConfirmation
        {
            Email = userDto.EMail,
            Id = Guid.NewGuid(),
            Token = userDto.Code,
        };
        
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserContext>();
        
        db.UserConfirmations.Add(confCode);
        await db.SaveChangesAsync();
        
        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/confirm-email", userDto);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        
        var user = db.Users.FirstOrDefault(user => user.AuthorizationParams.EMail == confCode.Email);
        
        Assert.NotNull(user);
        
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
    
    [Fact]
    public async Task ConfirmEmail_False_WrongCode()
    {
        // Arrange
        var userDto = new UserCreateDto
        {
            Code = "correctToken",
            EMail = "correctEmail@mail.ru",
            Password = "Abcabc123_123_"
        };
        var confCode = new UserConfirmation
        {
            Email = userDto.EMail,
            Id = Guid.NewGuid(),
            Token = "wrongCode",
        };
        
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserContext>();
        
        db.UserConfirmations.Add(confCode);
        await db.SaveChangesAsync();
        
        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/confirm-email", userDto);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        
        db.UserConfirmations.Remove(confCode);
        await db.SaveChangesAsync();
    }
    
    
    [Fact]
    public async Task ConfirmEmail_False_UserExist()
    {
        // Arrange
        var userDto = new UserCreateDto
        {
            Code = "correctToken",
            EMail = "correctEmail@mail.ru",
            Password = "Abcabc123_123_"
        };
        var confCode = new UserConfirmation
        {
            Email = userDto.EMail,
            Id = Guid.NewGuid(),
            Token = userDto.Code,
        };
        
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserContext>();
        
        db.UserConfirmations.Add(confCode);

        db.Users.Add(userDto.ToDomain());
        await db.SaveChangesAsync();
        
        // Act
        var result = await _client.PostAsJsonAsync("/api/auth/confirm-email", userDto);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        
        db.UserConfirmations.Remove(confCode);
        await db.SaveChangesAsync();
    }
}