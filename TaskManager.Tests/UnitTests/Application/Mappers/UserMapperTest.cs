using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Mappers;

public class UserMapperTest
{
    [Fact]
    public void UserMapperToDomain_IsChangedId()
    {
        var dto = new UserCreateDto
        {
            Code = "123",
            Password = "password",
            EMail = "1234",
            NickName = "NickName"
        };
        
        var user = dto.ToDomain();
        
        Assert.True(user.Id != Guid.Empty);
    }

    [Fact]
    public void UserMapperToDomain_IsHashigPassword()
    {
        var dto = new UserCreateDto()
        {
            Password = "password"
        };
        
        var user = dto.ToDomain();
        
        Assert.True(dto.Password != user.AuthorizationParams.PasswordHash);
    }
    [Fact]
    public void UserMapperApplyUpdate_IsHashigPassword()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "123",
            OldPassword = "password1",
            NewPassword = "password2"
        };
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams
            {
                EMail = "123@mail.ru",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            }
        };
        
        user.ApplyUpdate(dto);
        
        Assert.True(dto.NewPassword != user.AuthorizationParams.PasswordHash);
    }
}