using System.Net;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Mappers;
using TaskManager.Infrastructure.DB;
using Xunit;

namespace TaskManager.TaskManager.Tests.IntegrateTests.Auth;

public class Auth_LogIn_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public Auth_LogIn_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

    }
    [Fact]
    public async Task AuthLogIn_Success_CredentialsAreCorrect()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            EMail = "correctEMail@mail.ru",
            Password = "Abcabc123_123_"
        };
        
        // Act and Assert

        var response = await ActAndAssert(dto, async response =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.Equal("/work", result["redirect"]);

            // Проверяем, что Set-Cookie присутствует
            Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");

            var cookie = response.Headers.GetValues("Set-Cookie").First();
            Assert.Contains("TaskManagerAuth", cookie);
        });
        
        
    }

    private async Task<HttpResponseMessage> ActAndAssert(UserCreateDto dto, Action<HttpResponseMessage> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserContext>();

        var dtoNormal = new UserCreateDto
        {
            EMail = "correctEMail@mail.ru",
            Password = "Abcabc123_123_"
        };
        db.Users.Add(dtoNormal.ToDomain());
        await db.SaveChangesAsync();
        
        var response = await _client.PostAsJsonAsync("/postuser", dto);

        action(response);

        return response;
    }

    [Fact]
    public async Task AuthLogIn_False_PasswordAreInCorrect()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            EMail = "correctEMail@mail.ru",
            Password = "IncorrectPassword123_123_"
        };

        await ActAndAssert(dto, response => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode));
    }
    [Fact]
    public async Task AuthLogIn_False_EmailUnexist()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            EMail = "uncorrectEMail@mail.ru",
            Password = "Abcabc123_123_"
        };

        await ActAndAssert(dto, response => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode));
    }

    [Fact]
    public async Task AuthLogIn_False_FieldsEmpty()
    {
        // Arrange
        var dto = new UserCreateDto
        {
            EMail = "",
            Password = ""
        };

        await ActAndAssert(dto, response => Assert.Equal(HttpStatusCode.BadRequest
            , response.StatusCode));
    }
}