using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class NoteCreateDtoValidatorTests
{
    private readonly NoteCreatedDtoValidator _validator = new ();

    [Fact]
    public void NoteCreateDtoValidatorIsValid_True_WhenCredentialsIsValid()
    {
        var dto = new NoteCreateDto()
        {
            HeadOfNote = "123",
            BodyOfNote = "123",
            DateOfDeadLine = DateTime.Now+TimeSpan.FromHours(1),
            Images = new List<IFormFile>()
        };
        
        var result = _validator.Validate(dto);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NoteCreateDtoValidatorIsValid_False_WhenHeadIsNull()
    {
        var dto = new NoteCreateDto()
        {
            BodyOfNote = "123",
            DateOfDeadLine = DateTime.Now+TimeSpan.FromHours(1),
            Images = new List<IFormFile>()
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void NoteCreateDtoValidatorIsValid_False_WhenHeadInvalid()
    {
        var dto = new NoteCreateDto()
        {
            HeadOfNote = new string('a', 201),
            BodyOfNote = "123",
            DateOfDeadLine = DateTime.Now+TimeSpan.FromHours(1),
            Images = new List<IFormFile>()
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void NoteCreateDtoValidatorIsValid_False_WhenBodyIsNull()
    {
        var dto = new NoteCreateDto()
        {
            HeadOfNote = "1223",
            DateOfDeadLine = DateTime.Now+TimeSpan.FromHours(1),
            Images = new List<IFormFile>()
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void NoteCreateDtoValidatorIsValid_False_WhenInvalidDeadLine()
    {
        var dto = new NoteCreateDto()
        {
            HeadOfNote = "1223",
            BodyOfNote = "123",
            DateOfDeadLine = DateTime.Now-TimeSpan.FromDays(1),
            Images = new List<IFormFile>()
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void NoteCreateDtoValidatorIsValid_False_WhenImagesSoMuch()
    {
        var images = new List<IFormFile>();
        for (var i = 0; i < 12; i++)
        {
            images.Add(CreateFormFile("file"+ i, "image/jpeg"));
        }
        
        var dto = new NoteCreateDto()
        {
            HeadOfNote = "1223",
            BodyOfNote = "123",
            DateOfDeadLine = DateTime.Now+TimeSpan.FromHours(1),
            Images = images
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    private IFormFile CreateFormFile(string fileName, string contentType, string content = "fake image data")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}