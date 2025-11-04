using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappers;
using TaskManager.Application.Validators;
using TaskManager.Domain.ValueObjects;
using IAuthorizationService = TaskManager.Application.Interfaces.IAuthorizationService;

namespace TaskManager.Presentation.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        //получаем статус указанного профиля относительно авторизованного пользователя
        app.MapGet("/api/user/status/{profileId:guid}", [Authorize] async (Guid profileId, HttpContext context, 
            ILogger<Program> logger, IUserProfileService userProfileService) =>
        {
            logger.LogInformation("Execute endpoint /api/user/status/profileId");

            //получаем статус относительно авторизованного пользователя
            var status = await userProfileService.GetProfileStatusByID(context, profileId);
            
            return Results.Json(status);
        });
        
        //получаем профиль по его id
        app.MapGet("/api/user/profile/{id:guid}", [Authorize] async (Guid id, IUserRepository userRepository,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/user/profile/id");
            
            var user = await userRepository.GetUserByID(id);
            
            if (user != null)
                return Results.Json(user.ToReadDto());
            return Results.NotFound();
        });
        
        //получаем профиль авторизованного пользователя
        app.MapGet("/api/user/profile", [Authorize] async (HttpContext context, IUserRepository userRepo) =>
        {
            //получение авторизованного пользователя 
            var user = await userRepo.GetUserByID(Guid.Parse(context.User.Identity.Name));
            
            return Results.Json(user.ToReadDto());
        });

        // Обновляем профиль авторизованного пользователя
        app.MapPut("/api/user/stats", [ValidateAntiForgeryToken] [Authorize] async (HttpContext context,
            [FromForm] UserProfileUpdateDto updateDto, UserStatsUpdateDtoValidator validator, ILogger<Program> logger,
            IUserProfileService userProfileService) =>
            {
                logger.LogInformation("Execute endpoint /api/user/stats PUT");
                
                // Валидация входных данных
                var validRes = validator.Validate(updateDto);
                if (!validRes.IsValid)
                    return Results.BadRequest(validRes.Errors.Select(e => e.ErrorMessage));

                //Обновляем профиль авторизованного пользователя
                try
                {
                    await userProfileService.UpdateProfile(context, updateDto);
                }
                catch (UnauthorizedAccessException e)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok();
            });

        // Обновляем параметры авторизации пользователя
        app.MapPut("/api/user/", [ValidateAntiForgeryToken][Authorize] async (HttpContext context,
            UserAuthParamsUpdateDto authParamsUpdateDto, UserUpdateDtoValidator validator,
            IAuthorizationService authorizationService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/user/ PUT");
            
            //Валидируем входные данные
            var validRes = validator.Validate(authParamsUpdateDto);
            if (!validRes.IsValid)
                return Results.BadRequest(validRes.Errors.Select(e => e));

            // пробуем обновить входные параметры пользователя
            try
            {
                await authorizationService.UpdateAuthParams(context, authParamsUpdateDto);
            }
            catch (UnauthorizedAccessException e)
            {
                return Results.Unauthorized();
            }
            
            return Results.Ok();
        });
    }
}