using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;

namespace TaskManager.Presentation.Endpoints;

public static class FriendEndpoints
{
    public static void MapFriendEndpoints(this IEndpointRouteBuilder app)
    {
        // попытка получить всех друзей авторизованного пользователя
        app.MapGet("/api/friends", [Authorize] async (HttpContext context, IFriendServise friendServise,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/friends");
            
            // пробуем получить всех друзей пользователя
            var friends =  await friendServise.GetAllMyFriends(context);
            
            return Results.Json(friends);
        });
        
        // попытка удалить друга по его id у авторизованного пользователя
        app.MapDelete("/api/friends/remove/{id:guid}", [Authorize] async (Guid id, HttpContext context, 
            IFriendServise friendServise, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/friends/remove/id");

            //пробуем удалить друга из списка друзей авторизованного пользователя
            try
            {
                await friendServise.RemoveFriend(context, id);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }
            
            return Results.Ok();
        });
        
        // попытка отправить запрос дружбы пользователю по его id
        app.MapPost("/api/friends/request/{profileId:guid}", [Authorize] async (Guid profileId, HttpContext context,
            IFriendServise  friendServise, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/friends/request/profileId");
            
            //пробуем отправить заявку на дружбу
            try
            {
                await friendServise.SendFriendRequestAsync(context, profileId);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }
            
            return Results.Ok();
        });
        
        // попытка заблокировать пользователя по его id
        app.MapPost("/api/friends/block/{profileId:guid}", [Authorize] async (Guid profileId, HttpContext context,
            IFriendServise friendServise, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/friends/block/profileId");

            //пробуем заблокировать пользователя
            try
            {
                await friendServise.BlockUser(context, profileId);
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
            
            return Results.Ok();
        });
        
        // попытка разблокировать пользователя по его id
        app.MapPost("/api/friends/unblock/{profileId:guid}" , [Authorize] async (Guid profileId, HttpContext context,
            ILogger<Program> logger, IFriendServise friendServise) =>
        {
            logger.LogInformation("Execute endpoint /api/friends/unblock/profileId");
            
            //пробуем разблокировать пользователя
            try
            {
                await friendServise.UnblockUser(context, profileId);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok();
        });
        
        // попытка отменить заявку в друзья
        app.MapPost("/api/friends/cancel/{profileId:guid}", [Authorize] async (Guid profileId, HttpContext context,
            ILogger<Program> logger, IFriendServise friendServise) =>
        {
            logger.LogInformation("Execute endpoint /api/friends/cancel/profileId");

            // пробуем отменить отправленную заявку в друзья
            try
            {
                await friendServise.CanselFriendApplication(context, profileId);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok();
        });
        
        // попытка поиска пользователя по его никнейму
        app.MapGet("/api/user/search", [Authorize] async (string? query, IUserRepository userRepository,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/user/search");
            
            //валидация входных данных
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest("Query is required");

            // поиск пользователя по его никнейму
            var users = await userRepository.SearchUsersAsync(query);

            // Если ничего не найдено — возвращаем пустой список
            return Results.Json(users.Select(u => u.ToSearchReadDto())); 
        });
    }
}