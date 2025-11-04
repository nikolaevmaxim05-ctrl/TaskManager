using Moq;
using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Services;

public class ChatServiceTests
{
    private readonly Mock<ILogger<ChatService>> _logger;
    private readonly Mock<IChatRepository>  _chatRepo;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IMessageRepository> _messageRepo;
    private readonly ChatService _chatService;
    public ChatServiceTests()
    {
        _logger = new Mock<ILogger<ChatService>>();
        _chatRepo = new Mock<IChatRepository>();
        _userRepo = new Mock<IUserRepository>();
        _messageRepo = new Mock<IMessageRepository>();
        _chatService = new ChatService(_logger.Object, _chatRepo.Object, _userRepo.Object, _messageRepo.Object);
    }

    [Fact]
    public async Task GetAllMyChats_CorrectReturned_WhenCredentialsAreValid()
    {
        var context = new Mock<HttpContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams{EMail = "123"}
        };
        var chats = new List<Chat>
        {
            new Chat
            {
                Members = new List<User>{ user }
            }
        };
        
        user.Chats.Add(chats[0]);
        
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _chatRepo.Setup(repo => repo.ReadAllByUserId(user.Id)).ReturnsAsync(chats);

        var chatsDto = new List<ChatReadDto>();
        chats.ForEach(chat => chatsDto.Add(chat.ToReadDto()));
        
        var result = await _chatService.GetAllMyChatsAsync(context.Object);
        
        Assert.Equal(1, result.Count);
        Assert.Equal(chatsDto[0].Id, result[0].Id);
    }
    [Fact]
    public async Task GetChatsById_CorrectReturned_WhenCredentialsAreValid()
    {
        var context = new Mock<HttpContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthorizationParams = new AuthorizationParams
            {
                EMail = "123"
            }
        };
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Members = new List<User>
            {
                user
            }
        };
        user.Chats.Add(chat);
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _userRepo.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _chatRepo.Setup(repo => repo.ReadByChatId(chat.Id)).ReturnsAsync(chat);

        var chatDto = chat.ToReadDto();
        
        var result = await _chatService.GetChatByIdAsync(context.Object, chat.Id);
        
        Assert.Equal(chatDto.Id, result.Id);
    }

    [Fact]
    public async Task SendMessage_CorrectReturned_WhenCredentialsAreValid()
    {
       
        var chat = new Chat
        {
            Id = Guid.NewGuid()
        };
        var dto = new MessageCreateDto();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Chats = new List<Chat>
            {
                chat
            },
            AuthorizationParams = new AuthorizationParams{ EMail = "123"}
        };
        chat.Members.Add(user);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _userRepo.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _chatRepo.Setup(repo => repo.ReadByChatId(chat.Id)).ReturnsAsync(chat);
        
        await _chatService.SendMessage(context.Object, chat.Id, dto);
        
        _messageRepo.Verify(repo => repo.Create(It.IsAny<Message>()), Times.Once);
        _chatRepo.Verify(repo => repo.Update(It.IsAny<Chat>()), Times.Once);
    }
    
    [Fact]
    public async Task EditMessage_CorrectReturned_WhenCredentialsAreValid()
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid()
        };
        var dto = new MessageUpdateDto
        {
            Id = Guid.NewGuid()
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Chats = new List<Chat>
            {
                chat
            },
            AuthorizationParams = new AuthorizationParams { EMail = "123"}
        };
        chat.Members.Add(user);
       
        
        var message = new Message()
        {
            Id = dto.Id,
            Chat = chat,
            ChatId = chat.Id,
            Sender = user,
            SenderId = user.Id,
        };
        chat.Messages.Add(message);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _userRepo.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _chatRepo.Setup(repo => repo.ReadByChatId(chat.Id)).ReturnsAsync(chat);
        _messageRepo.Setup(repo => repo.Read(message.Id)).ReturnsAsync(message);
        
        await _chatService.EditMessage(context.Object, chat.Id, dto);
        
        _messageRepo.Verify(repo => repo.Update(It.IsAny<Message>()), Times.Once);
        _chatRepo.Verify(repo => repo.Update(It.IsAny<Chat>()), Times.Once);
    }
    
    [Fact]
    public async Task EditMessage_BadHttpRequestException_WhenCredentialsAreNotValid()
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid()
        };
        var dto = new MessageUpdateDto
        {
            Id = Guid.NewGuid()
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Chats = new List<Chat>
            {
                chat
            },
            AuthorizationParams = new AuthorizationParams { EMail = "123"}
        };
        chat.Members.Add(user);
       
        
        var message = new Message()
        {
            Id = dto.Id,
            Chat = chat,
            ChatId = chat.Id,
            Sender = user,
            SenderId = user.Id,
        };
        chat.Messages.Add(message);
        
        dto.Id = Guid.NewGuid();

      
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _userRepo.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _chatRepo.Setup(repo => repo.ReadByChatId(chat.Id)).ReturnsAsync(chat);
        
        
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _chatService.EditMessage(
            context.Object, chat.Id, dto));
        
        _messageRepo.Verify(repo => repo.Update(It.IsAny<Message>()), Times.Never);
        _chatRepo.Verify(repo => repo.Update(It.IsAny<Chat>()), Times.Never);
    }
    [Fact]
    public async Task DeleteMessage_CorrectReturned_WhenCredentialsAreValid()
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid()
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Chats = new List<Chat>
            {
                chat
            },
            AuthorizationParams = new AuthorizationParams{EMail = "123"}
        };
        var message = new Message()
        {
            Id = Guid.NewGuid(),
            ChatId = chat.Id,
            Sender = user,
            SenderId = user.Id
        };
        chat.Members.Add(user);
        chat.Messages.Add(message);
        
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _userRepo.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _chatRepo.Setup(repo => repo.ReadByChatId(chat.Id)).ReturnsAsync(chat);
        
        await _chatService.DeleteMessage(context.Object, chat.Id, message.Id);
        
        _messageRepo.Verify(repo => repo.Delete(It.IsAny<Message>()), Times.Once);
        _chatRepo.Verify(repo => repo.Update(It.IsAny<Chat>()), Times.Once);
    }
    [Fact]
    public async Task DeleteMessage_BadHttpRequestException_WhenCredentialsAreNotValid()
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid()
        };
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Chats = new List<Chat>
            {
                chat
            },
            AuthorizationParams = new AuthorizationParams{EMail = "123"}
        };
        chat.Members.Add(user);

        var message = new Message
        {
            Id = Guid.NewGuid(),
        };

      
        var context = new Mock<HttpContext>();
        context.Setup(context => context.User.Identity.Name).Returns(user.Id.ToString());
        _userRepo.Setup(repo => repo.GetUserByID(user.Id)).ReturnsAsync(user);
        _chatRepo.Setup(repo => repo.ReadByChatId(chat.Id)).ReturnsAsync(chat);
        
        
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _chatService.DeleteMessage(context.Object, 
            chat.Id, message.Id));
        
        _messageRepo.Verify(repo => repo.Delete(It.IsAny<Message>()), Times.Never);
        _chatRepo.Verify(repo => repo.Update(It.IsAny<Chat>()), Times.Never);
    }
}