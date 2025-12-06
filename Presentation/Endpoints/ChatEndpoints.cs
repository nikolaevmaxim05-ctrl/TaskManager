using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Validators.MessageValidators;

namespace TaskManager.Presentation.Endpoints;

public static class 
    
    ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        // попытка получить все чаты авторизованного пользователя
        app.MapGet("/api/myChats", [Authorize] async (HttpContext context, IChatService chatService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/myChats");
            
            //получение всех чатов авторизованного пользователя
            var chatsDto = await chatService.GetAllMyChatsAsync(context);
             
            return Results.Json(chatsDto);
        });
        
        // получение конкретного чата по его ID
        app.MapGet("api/chat/{id:guid}", [Authorize] async (Guid id, HttpContext context, ILogger<Program> logger,
            IChatService chatService) =>
        {
            logger.LogInformation("Execute endpoint /api/chat/id");

            //получаем чат по его id
            var chat = new ChatReadDto();
            try
            {
                chat = await chatService.GetChatByIdAsync(context, id);
            }
            catch (BadHttpRequestException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            
            return Results.Json(chat);
        });
        
        // попытка отправки сообщения в чат
        app.MapPost("api/chat/{id:guid}/sendMessage", [Authorize] [ValidateAntiForgeryToken] async (Guid id, 
            [FromForm] MessageCreateDto dto, HttpContext context, ILogger<Program> logger, IChatService chatService,
            MessageCreateDtoValidator validator) =>
        {
            logger.LogInformation("Execute endpoint /api/chat/id/sendMessage");
            
            //валидируем входные данные
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                logger.LogError("При попытке отправления сообщения в чат были, обнаружены не валидные входные данные");
                return Results.BadRequest(validationResult.Errors);
            }

            //пробуем отправить сообщение в чат
            try
            {
                await chatService.SendMessage(context, id, dto);
            }
            catch (BadHttpRequestException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            
            return Results.Ok();
        });
        
        // попытка редактирования сообщения в чате
        app.MapPut("api/chat/{id:guid}/redactMessage", [Authorize] [ValidateAntiForgeryToken] async (Guid id,
            [FromForm] MessageUpdateDto dto, HttpContext context,IChatService service ,MessageUpdateDtoValidator validator,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/chat/id/redactMessage");
            
            //валидируем входные данные
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                logger.LogError("При попытке редактирования сообщения, на вход методу посупили невалидные данные");
                return Results.BadRequest(validationResult.Errors);
            }
            
            //пробуем редактировать сообщение в чате
            try
            {
                await service.EditMessage(context, id, dto);
            }
            catch (BadHttpRequestException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }

            return Results.Ok();

        });
        
        // попытка удаления сообщения из чата
        app.MapDelete("api/chat/{id:guid}/deleteMessage", [Authorize] async (Guid id, [FromForm] Guid messageId,
            HttpContext context, ILogger<Program> logger, IChatService chatService) =>
        {
            logger.LogInformation("Execute endpoint /api/chat/id/deleteMessage");
            
            //пытаемся удалить сообщение
            try
            {
                await chatService.DeleteMessage(context, id, messageId);
            }
            catch (BadHttpRequestException e)
            {
                return Results.NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }

            return Results.Ok();
        });
    }
}