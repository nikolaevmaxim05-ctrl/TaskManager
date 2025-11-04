using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.Validators.MessageValidators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class MessageCreateDtoValidatorTests
{
    private readonly MessageCreateDtoValidator _validator = new();

    [Fact]
    public void MessageCreateDtoValidatorIsValid_True_WhenCredentialsAreValid()
    {
        var dto = new MessageCreateDto
        {
            Body = "sdhfglkjsdh",
            Sender = Guid.NewGuid()
        };
        
        var result = _validator.Validate(dto);
        
        Assert.True(result.IsValid);
    }
    [Fact]
    public void MessageCreateDtoValidatorIsValid_False_WhenBodyIsEmpty()
    {
        var dto = new MessageCreateDto
        {
            Sender = Guid.NewGuid(),
            Body = ""
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void MessageCreateDtoValidatorIsValid_False_WhenBodyIsOver1000()
    {
        var dto = new MessageCreateDto
        {
            Sender = Guid.NewGuid(),
            Body = new string('a', 1002),
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void MessageCreateDtoValidatorIsValid_False_WhenIdIsEmpty()
    {
        var dto = new MessageCreateDto
        {
            Sender = Guid.Empty,
            Body = "123",
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
}