using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class UserConfirmationCreateDtoValidatorTests
{
    private readonly UserConfirmationCreateDtoValidator _validator = new ();

    [Fact]
    public void UserConfirmationCreateDtoIsValid_True_When_Credentials_Are_Valid()
    {
        var dto = new UserConfirmationsCreateDto
        {
            EMail = "biba@mail.ru"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.True(result.IsValid);
    }
    [Fact]
    public void UserConfirmationCreateDtoIsValid_False_When_EmailIsNull()
    {
        var dto = new UserConfirmationsCreateDto
        {
            EMail = null
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserConfirmationCreateDtoIsValid_False_When_EmailAreNotCorrect()
    {
        var dto = new UserConfirmationsCreateDto
        {
            EMail = "123123"
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
}