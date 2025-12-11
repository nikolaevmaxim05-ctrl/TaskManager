using System.Net;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Mappers;
using TaskManager.Infrastructure.DB;
using Xunit;

namespace TaskManager.TaskManager.Tests.IntegrateTests.Auth;

public class Auth_Logout_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public Auth_Logout_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Auth_Logout_Works()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserContext>();

        var dto = new UserCreateDto
        {
            EMail = "correctEMail@mail.ru",
            Password = "Abcabc123_123_"
        };
        db.Users.Add(dto.ToDomain());
        db.SaveChanges();
        
        var status = await _client.PostAsJsonAsync("/postuser", dto);
        
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        
        await _client.GetAsync("/logout");
        
        status = await _client.GetAsync("/api/me");
        
        Assert.Equal(HttpStatusCode.Unauthorized, status.StatusCode);
    }
}