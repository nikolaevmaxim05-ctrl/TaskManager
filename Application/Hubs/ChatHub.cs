using Microsoft.AspNetCore.SignalR;

namespace TaskManager.Application.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        string chatId = Context.GetHttpContext().Request.Query["chatId"];

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);

        await base.OnConnectedAsync();
        
        _logger.LogInformation("SignalR Connected");
    }
}