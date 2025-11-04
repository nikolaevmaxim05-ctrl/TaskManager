using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class UserUpdateDtoValidatorTests
{
    private readonly UserUpdateDtoValidator _validator = new();

    [Fact]
    public void UserUpdateDtoValidatorIsValid_True_WhenCredentialsIsValid()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "bom@gmail.com",
            NewPassword = "Qw3!rT8@",
            OldPassword = "M9$kL2#p"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.True(result.IsValid);
    }
    [Fact]
    public void UserUpdateDtoValidatorIsValid_False_WhenEmailIsNull()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = null,
            NewPassword = "Qw3!rT8@",
            OldPassword = "M9$kL2#p"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserUpdateDtoValidatorIsValid_False_WhenEmailIsInvalid()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "123",
            NewPassword = "Qw3!rT8@",
            OldPassword = "M9$kL2#p"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserUpdateDtoValidatorIsValid_False_WhenNewPasswordIsNull()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "bom@mail.ru",
            NewPassword = null,
            OldPassword = "M9$kL2#p"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void UserUpdateDtoValidatorIsValid_False_WhenOldPasswordIsNull()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "bom@mail.ru",
            NewPassword = "M9$kL2#p",
            OldPassword = null
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserUpdateDtoValidatorIsValid_False_WhenOldPasswordIsInvalid()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "bom@mail.ru",
            NewPassword = "M9$kL2#p",
            OldPassword = "123"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserUpdateDtoValidatorIsValid_False_WhenNewPasswordIsInvalid()
    {
        var dto = new UserAuthParamsUpdateDto
        {
            EMail = "bom@mail.ru",
            NewPassword = "123",
            OldPassword = "M9$kL2#p"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
}