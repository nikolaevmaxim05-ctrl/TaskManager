using Microsoft.AspNetCore.SignalR;
using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Hubs;

public class ChatHubContext : IChatHub
{
    private readonly ILogger<ChatHubContext> _logger;
    private readonly IHubContext<ChatHub> _hub;

    public ChatHubContext(ILogger<ChatHubContext> logger, IHubContext<ChatHub> hub)
    {
        _logger = logger;
        _hub = hub;
    }
    public async Task SendMessage(Message message)
    {
        _logger.LogInformation("SignalR Receiving Message");
        await _hub.Clients.Group(message.Chat.Id.ToString()).SendAsync("ReceiveMessage", message.ToReadDto());
    }

    public async Task UpdateMessage(Message message)
    {
        _logger.LogInformation("SignalR Updating Message");
        await _hub.Clients.Group(message.Chat.Id.ToString()).SendAsync("UpdateMessage", message.ToReadDto());
    }

    public async Task DeleteMessage(Message message)
    {
        _logger.LogInformation("SignalR Deleting Message");
        await _hub.Clients.Group(message.ChatId.ToString()).SendAsync("DeleteMessage", message.ToReadDto());
    }
}