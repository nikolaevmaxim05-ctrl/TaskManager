using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Domain.Entities;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Validators;
using IAuthorizationService = TaskManager.Application.Interfaces.IAuthorizationService;

namespace TaskManager.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // выход из аккаунта
        app.MapGet("/logout", async (HttpContext context,  ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /logout");
            
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
        
        //проверка авторизации, если не авторизован, то редирект на страницу входа
        app.MapGet("/check-auth", [Authorize](ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /check-auth");
            
            return Results.Redirect("/login");
        });
        
        //попытка входа в аккаунт
        app.MapPost("/postuser", [ValidateAntiForgeryToken] async  (UserCreateDto userCreateDto,
            UserDtoValidator validator, HttpContext context, IAuthorizationService autorizationService, 
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /postuser");
            
            // валидация входных данных
            var validationResult = await validator.ValidateAsync(userCreateDto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            //попытка входа в аккаунт
            var user = await autorizationService.LogIn(userCreateDto);
            if (user == null)
                return Results.Unauthorized();

            //устанавливаем клаймы
            autorizationService.SetClaims(context, user.Id);
            
            

            return Results.Ok(new { redirect = "/work" });
        });
        
        // попытка зарегаться
        app.MapPost("/api/auth/confirm-email", [ValidateAntiForgeryToken] async (UserCreateDto userCreateDto, 
            UserDtoValidator validator,string? returnUrl, HttpContext context, IAuthorizationService autorizationService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/auth/confirm-email");
            
            //валидация входных данных
            var validationResult = await validator.ValidateAsync(userCreateDto);
            if (!validationResult.IsValid)
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            
            // пытаемся зарегаться
            var user = new User();
            try
            {
                user = await autorizationService.SignIn(userCreateDto);
            }
            catch (BadHttpRequestException e)
            {
                user = null;
                return Results.BadRequest("Неверный код");
            }
            catch (DuplicateNameException e)
            {
                user = null;
                return Results.Conflict($"Пользователь с email {userCreateDto.EMail} уже существует");
            }
            
            //создаем клеймы и входим в систему
            if (user != null) 
                autorizationService.SetClaims(context, user.Id);
                
            return Results.Ok(new { redirect = "/work" });
        });
        
        //проверка аутентификации
        app.MapGet("/api/me", (HttpContext context, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/me");
            
            if (context.User?.Identity?.IsAuthenticated == true)
                return Results.Ok();
            else
                return Results.Unauthorized();
        });

        // получение id авторизованного пользователя
        app.MapGet("/api/user/me", [Authorize](HttpContext context, ILogger<Program> logger) =>
        {
            logger.LogInformation("Execute endpoint /api/user/me");
            
            var id = context.User.Identity.Name;
            return Results.Json(id);
        });
        
        //попытка отправить новый код подтверждения
        app.MapPost("/api/auth/send-confirm-code", [ValidateAntiForgeryToken] async
            ([FromBody]UserConfirmationsCreateDto dto, ILogger<Program> logger, 
                IUserConfirmationService userConfirmationService) =>
        {
            logger.LogInformation("Execute endpoint /api/auth/send-confirm-code");
            
            //пробуем отправить код подтверждения на почту пользователю
            try
            {
                await userConfirmationService.SendConfirmationEmail(dto);
            }
            catch (BadHttpRequestException e)
            {
                return Results.BadRequest("Пользователь с таким email уже существует");
            }
            
            return Results.Ok();
        });
    }
}