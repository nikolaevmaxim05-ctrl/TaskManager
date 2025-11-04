using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class UserDtoValidatorTests
{
    private readonly UserDtoValidator _validator = new ();

    [Fact]
    public void UserCreateDtoIsValid_True_WhenCredentialsIsValid()
    {
        var dto = new UserCreateDto
        {
            EMail = "123@mail.ru",
            Password = @"Aa1!bcde\n"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.True(result.IsValid);
    }
    [Fact]
    public void UserCreateDtoIsValid_False_WhenEmailIsNull()
    {
        var dto = new UserCreateDto
        {
            EMail = null,
            Password = @"Aa1!bcde\n"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserCreateDtoIsValid_False_WhenPasswordIsNull()
    {
        var dto = new UserCreateDto
        {
            EMail = "123@mail.ru",
            Password = null
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserCreateDtoIsValid_False_WhenEmailIsInvalid()
    {
        var dto = new UserCreateDto
        {
            EMail = "123456",
            Password = @"Aa1!bcde\n"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserCreateDtoIsValid_False_WhenPasswordIsInvalid()
    {
        var dto = new UserCreateDto
        {
            EMail = "123@mail.ru",
            Password = "123456"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
}