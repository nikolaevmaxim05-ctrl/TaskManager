using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class NoteServiceTests
{
    private readonly Mock<ILogger<NoteService>> _logger;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<INoteRepository> _noteRepository;
    private readonly Mock<IImageService> _imageService;
    private readonly Mock<IWebHostEnvironment> _environment;
    private readonly NoteService _service;

    public NoteServiceTests()
    {
        _logger = new Mock<ILogger<NoteService>>();
        _userRepository = new Mock<IUserRepository>();
        _noteRepository = new Mock<INoteRepository>();
        _imageService = new Mock<IImageService>();
        _environment = new Mock<IWebHostEnvironment>();
        
        _service = new NoteService(_logger.Object, _userRepository.Object, _noteRepository.Object, _imageService.Object, 
            _environment.Object);
    }
    
    [Fact]
    public async Task GetAllMyNotes_ReturnListNotes_WhenCredentialAreValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams {EMail = "123"}
        };
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
        };
        user.NotePool.Add(note);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);

        var result = await _service.GetAllMyNoteAsync(context.Object);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(note.Id, result[0].Id);
    }
    
    [Fact]
    public async Task GetNoteById_NoteReadDto_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams {EMail = "123"}
        };
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
        };
        user.NotePool.Add(note);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _noteRepository.Setup(repo => repo.Read(note.Id)).ReturnsAsync(note);

        var result = await _service.GetNoteByIdAsync(context.Object, note.Id);
        
        Assert.NotNull(result);
        Assert.Equal(note.Id, result.Id);
    }
    [Fact]
    public async Task GetNoteById_BadHttpRequest_WhenNoteIdAreInvalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
        };
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
        };
        user.NotePool.Add(note);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _noteRepository.Setup(repo => repo.Read(note.Id)).ReturnsAsync(note);

        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _service.GetNoteByIdAsync(context.Object, 
            Guid.NewGuid()));
    }
    [Fact]
    public async Task GetNoteById_UnauthorizeAccessException_WhenNoteAreInvalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
        };
        var note = new Note
        {
            Id = Guid.NewGuid(),
        };
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _noteRepository.Setup(repo => repo.Read(note.Id)).ReturnsAsync(note);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.GetNoteByIdAsync(context.Object, 
            note.Id));
    }
    
    [Fact]
    public async Task DeleteNoteByIdAsync_DeletingNote_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams {EMail = "123"}
        };
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = user.Id
        };
        user.NotePool.Add(note);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        
        _userRepository.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _noteRepository.Setup(repo => repo.Read(note.Id)).ReturnsAsync(note);
        _noteRepository.Setup(repo => repo.Delete(It.IsAny<Note>())).ReturnsAsync(true);
        
        await _service.DeleteNoteByIdAsync(context.Object, note.Id);
        
        _noteRepository.Verify(repo => repo.Delete(It.IsAny<Note>()), Times.Once);
    }
}