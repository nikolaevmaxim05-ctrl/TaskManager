    using System.Net;
    using TaskManager.Application.DTOs.UserConfirmationsDTOs;
    using TaskManager.Application.DTOs.UserDTOs;
    using TaskManager.Application.Mappers;
    using TaskManager.Infrastructure.DB;
    using Xunit;

    namespace TaskManager.TaskManager.Tests.IntegrateTests.Auth;

    public class Auth_SendConfirmCode_Tests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public Auth_SendConfirmCode_Tests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Auth_SendConfirmCode_Success_CredentialsAreCorrect()
        {
            var codeDto = new UserConfirmationsCreateDto
            {
                EMail = "newEmail@mail.ru"
            };

            await _client.PostAsJsonAsync("/api/auth/send-confirm-code", codeDto);
            
            
            await using var scope = _factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<UserContext>();
            
            var confirmation = db.UserConfirmations.FirstOrDefault(c => c.Email == codeDto.EMail);
            
            Assert.Equal(confirmation.Email, codeDto.EMail);
        }
        
        [Fact]
        public async Task Auth_SendConfirmCode_False_EmailIsExist()
        {
            var codeDto = new UserConfirmationsCreateDto
            {
                EMail = "oldEmail@mail.ru"
            };
            await using var scope = _factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<UserContext>();

            var oldUser = new UserCreateDto
            {
                EMail = "oldEmail@mail.ru",
                Password = "oldPassword"
            };
            db.Users.Add(oldUser.ToDomain());
            await db.SaveChangesAsync();

            var result = await _client.PostAsJsonAsync("/api/auth/send-confirm-code", codeDto);
            
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }