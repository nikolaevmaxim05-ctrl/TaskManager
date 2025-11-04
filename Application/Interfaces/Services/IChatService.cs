using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IChatService
{
    public Task<List<ChatReadDto>> GetAllMyChatsAsync(HttpContext context); 
    public Task<ChatReadDto> GetChatByIdAsync(HttpContext context, Guid id);
    public Task SendMessage (HttpContext context, Guid chatId, MessageCreateDto message);
    public Task EditMessage (HttpContext context, Guid chatId, MessageUpdateDto message);
    public Task DeleteMessage (HttpContext context, Guid chatId, Guid messageId);
}