using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.Validators.MessageValidators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class MessageUpdateDtoValidatorTests
{
    private readonly MessageUpdateDtoValidator _messageUpdateDtoValidator = new();

    [Fact]
    public void MessageUpdateDtoValidatorIsValid_True_WhenCredentialsAreValid()
    {
        var dto = new MessageUpdateDto
        {
            Id = Guid.NewGuid(),
            Body = "sdhfglkjsdh"
        };
        
        var result = _messageUpdateDtoValidator.Validate(dto);
        
        Assert.True(result.IsValid);
    }
    [Fact]
    public void MessageUpdateDtoValidatorIsValid_False_WhenBodyIsEmpty()
    {
        var dto = new MessageUpdateDto
        {
            Id = Guid.NewGuid(),
            Body = ""
        };
        
        var result = _messageUpdateDtoValidator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void MessageUpdateDtoValidatorIsValid_False_WhenBodyIsOver1000()
    {
        var dto = new MessageUpdateDto
        {
            Id = Guid.NewGuid(),
            Body = new string('a', 1002),
        };
        
        var result = _messageUpdateDtoValidator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void MessageUpdateDtoValidatorIsValid_False_WhenIdIsEmpty()
    {
        var dto = new MessageUpdateDto
        {
            Id = Guid.Empty,
            Body = "123",
        };
        
        var result = _messageUpdateDtoValidator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
}