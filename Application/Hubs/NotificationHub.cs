using Microsoft.AspNetCore.SignalR;

namespace TaskManager.Application.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        
        string channelId = Context.GetHttpContext().Request.Query["userId"];

        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        
        await base.OnConnectedAsync();
        
        _logger.LogInformation("SignalR Connected");
    }
}