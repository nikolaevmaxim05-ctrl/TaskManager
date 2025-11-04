using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Application.Mappers;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Mappers;

public class UserConfirmationMapperTests
{
    [Fact]
    public void UserConfirmationMapperToDomain_IsChangedId()
    {
        var dto = new UserConfirmationsCreateDto();
        
        var user = dto.ToDomain("123");
        
        Assert.True(user.Id != Guid.Empty);
    }
}