using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IAuthorizationService
{
    public Task<User?> LogIn(UserCreateDto userCreateDto);
    public Task<User?> SignIn(UserCreateDto userCreateDto);
    public Task<bool> CodeConfirmation(string token, string email);
    public Task SetClaims(HttpContext httpContext, Guid userId);
    public Task UpdateAuthParams(HttpContext httpContext, UserAuthParamsUpdateDto dto);
}