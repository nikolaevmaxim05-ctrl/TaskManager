using Moq;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class UserStatsUpdateDtoValidatorTests
{
    private readonly UserStatsUpdateDtoValidator _validator = new();

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
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_True_WhenCredentialIsValid()
    {
        var avatar = CreateFormFile("/avatar.jpg", "image/jpg");
        var dto = new UserProfileUpdateDto
        {
            NickName = "NickName",
            Avatar = avatar
        };
        
        var result = _validator.Validate(dto);
        
        Assert.True(result.IsValid);
    }
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_False_WhenNickNameIsNull()
    {
        var avatar = CreateFormFile("/avatar", "image/jpeg");
        var dto = new UserProfileUpdateDto
        {
            NickName = null,
            Avatar = avatar
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_False_WhenNickNameLenghtIsLower3()
    {
        var avatar = CreateFormFile("/avatar", "image/jpeg");
        var dto = new UserProfileUpdateDto
        {
            NickName = "5",
            Avatar = avatar
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_False_WhenNickNameLenghtIsUpper32()
    {
        var avatar = CreateFormFile("/avatar", "image/jpeg");
        var dto = new UserProfileUpdateDto
        {
            NickName = new string('0', 33),
            Avatar = avatar
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_False_WhenNickNameIsInvalid()
    {
        var avatar = CreateFormFile("/avatar", "image/jpeg");
        var dto = new UserProfileUpdateDto
        {
            NickName = "Bad Nickname!",
            Avatar = avatar
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_False_WhenAvatarIsInvalidLenght()
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(file => file.ContentType).Returns("image/jpeg");
        mock.Setup(file => file.Length).Returns(6 * 1024 * 1024);
        
        var dto = new UserProfileUpdateDto
        {
            NickName = "Nickname",
            Avatar = mock.Object
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void UserStatsUpdateDtoValidatorIsValid_False_WhenAvatarIsInvalidFormat()
    {
        var avatar = CreateFormFile("/avatar", "image/exe");
        var dto = new UserProfileUpdateDto
        {
            NickName = "Nickname",
            Avatar = avatar
        };
        
        var result = _validator.Validate(dto);
        
        Assert.False(result.IsValid);
    }
}