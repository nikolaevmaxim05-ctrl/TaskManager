using Microsoft.AspNetCore.SignalR;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Hubs;

public class NotificationHubContext : INotificationHub
{
    private readonly ILogger<NotificationHubContext> _logger;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationHubContext(ILogger<NotificationHubContext> logger,  IHubContext<NotificationHub> hub)
    {
        _logger = logger;
        _hub = hub;
    }


    public async Task SendNotification(Notification notification, Guid recipientId)
    {
        _logger.LogInformation("SignalR Sending Notification");
        
        //await _hub.Clients.User(recipientId.ToString()).SendAsync("ReceiveNotification", notification);
        await _hub.Clients.Group(recipientId.ToString()).SendAsync("ReceiveNotification", notification);
    }
}