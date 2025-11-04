using Moq;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Validators;

public class ImageFileValidatorTests
{
    private readonly ImageFileValidator _validator;

    public ImageFileValidatorTests()
    {
        _validator = new ImageFileValidator();
    }
    [Fact]
    public void ImageFileValidatorIsValid_True_WhenCredentialsIsValid()
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(file => file.ContentType).Returns("image/jpeg");
        mock.Setup(file => file.Length).Returns(100);
        
        var validationResult = _validator.Validate(mock.Object);
        
        Assert.True(validationResult.IsValid);
    }
    [Fact]
    public void ImageFileValidatorIsValid_False_WhenInvalidContentType()
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(file => file.ContentType).Returns("image/exe");
        mock.Setup(file => file.Length).Returns(100);
        
        var validationResult = _validator.Validate(mock.Object);
        
        Assert.False(validationResult.IsValid);
    }
    [Fact]
    public void ImageFileValidatorIsValid_False_WhenInvalidLength()
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(file => file.ContentType).Returns("image/jpeg");
        mock.Setup(file => file.Length).Returns(6 * 1024 * 1024);
        
        var validationResult = _validator.Validate(mock.Object);
        
        Assert.False(validationResult.IsValid);
    }
    [Fact]
    public void ImageFileValidatorIsValid_False_WhenFileIsNull()
    {
        var result = _validator.Validate(new Mock<IFormFile>().Object);

        Assert.False(result.IsValid);
    }
}