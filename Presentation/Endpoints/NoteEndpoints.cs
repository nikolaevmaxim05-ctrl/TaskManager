using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Validators;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Presentation.Endpoints;

public static class NoteEndpoints
{
    public static void MapNoteEndpoints(this IEndpointRouteBuilder app)
    {
        // попытка получить все задачи авторизованного пользователя
        app.MapGet("/api/tasks/", [Authorize] async (HttpContext context, INoteService noteService, 
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/tasks/");
            
            //пробуем получить все задачи пользователя
            var notePool = await noteService.GetAllMyNoteAsync(context);
            
            return Results.Json(notePool);
        });

        // попытка получения задачи по ее id
        app.MapGet("/api/tasks/{id:guid}", [Authorize] async (Guid id, HttpContext context, INoteService noteService, 
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/tasks/id");
            
            var noteReadDto = new NoteReadDto();
            // Пробуем получить задачу по ее id
            try
            {
                noteReadDto = await noteService.GetNoteByIdAsync(context, id);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            
            return Results.Json(noteReadDto);
        });
         
        // попытка удаления задачи по ее id
        app.MapDelete("/api/tasks/{id:guid}", [ValidateAntiForgeryToken] [Authorize] async (Guid id, 
            HttpContext context,  ILogger<Program> logger, INoteService noteService) =>
        {
            logger.LogInformation("Execute endpoint /api/tasks/id  DELETE");

            // пробуем удалить задачу по ее id
            try
            {
                await noteService.DeleteNoteByIdAsync(context, id);
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
        
        // попытка создания новой задачи
        app.MapPost("/api/tasks/create",  [ValidateAntiForgeryToken] [Authorize] 
            async ([FromForm]NoteCreateDto noteCreateDto, HttpContext context, NoteCreatedDtoValidator validator, 
            ILogger<Program> logger, INoteService noteService) =>
        {
            logger.LogInformation("Execute endpoint /api/tasks/create");

            // валидируем входные данные
            var validationResult = await validator.ValidateAsync(noteCreateDto);

            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            // пробуем создать новую задачу
            await noteService.CreateNoteAsync(context, noteCreateDto);
            
            return Results.Ok();
        });

        // попытка редактирования задачи
        app.MapPut("/api/tasks/{id:guid}", [ValidateAntiForgeryToken] async (Guid id, [FromForm] NoteUpdateDto noteDto, 
            HttpContext context, NoteUpdateDtoValidator validator, ILogger<Program> logger, INoteService noteService) =>
        {
            logger.LogInformation("Execute endpoint /api/tasks/id  PUT");
            
            //валидируем входные данные
            var validationResult = await validator.ValidateAsync(noteDto);
                
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            
            //пробуем обновить задачу
            try
            {
                await noteService.UpdateNoteAsync(context, noteDto, id);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            
            return Results.Ok();
        });

        // попытка получить статистику по задачам авторизированного пользователя
        app.MapGet("/api/tasks/stats/{id:guid}", [Authorize] async (Guid id,HttpContext context, INoteRepository noteRepo) =>
        {
            //запрос в бд, что бы достать все необходиммые данные
            var notes = await noteRepo.ReadAllByUserId(id);
            
            return Results.Json(new
            {
                total = notes.Count(),
                inProgress = notes.Count(note => note.Status == NoteStatus.InProgress),
                completed = notes.Count(note => note.Status == NoteStatus.Completed),
                overdue = notes.Count(note => note.Status == NoteStatus.Overdue),
            });
        });
    }
}