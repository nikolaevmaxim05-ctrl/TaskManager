using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.DB;

namespace TaskManager.Presentation.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        // попытка принять запрос в друзья через уведомление
        app.MapPost("api/notifications/acceptFriendRequest/{id:guid}", [Authorize] async (Guid id, 
            HttpContext context, IFriendServise friendServise, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint api/notifications/acceptFriendRequest/id:guid");

            // Пробуем принять запрос в друзья
            try
            {
                await friendServise.AcceptFriendRequest(context, id);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok();
        });
        
        // попытка получить все уведомления авторизованного пользователя
        app.MapGet("api/notifications", [Authorize] async (HttpContext context, IUserRepository userRepository,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint api/notifications");
            
            var user = await userRepository.GetUserByID(Guid.Parse(context.User.Identity.Name));
            var notifications = user.Notifications;

            return Results.Json(notifications);
        });

        // попытка отклонить запрос в друзья через уведомление
        app.MapPost("api/notifications/dismissFriendRequest/{id:guid}", [Authorize] async (Guid id, 
            HttpContext context, IFriendServise friendServise, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint api/notifications/dismissFriendRequest/id");
            
            //пробуем отклонить запрос в друзья
            try
            {
                await friendServise.DismissFriendRequest(context, id);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }
            
            return Results.Ok();
        });
        
        // попытка удалить уведомление
        app.MapDelete("api/notifications/{id:guid}", [Authorize] async (Guid id,  HttpContext context, 
            INotificationService notificationService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint api/notifications/id DELETE");

            //пробуем удалить уведомление
            try
            {
                await notificationService.DeleteNotificationAsync(context, id);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }
            
            return Results.Ok();
        });
    }
}