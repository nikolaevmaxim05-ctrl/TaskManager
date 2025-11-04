using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Interfaces;

public interface IUserProfileService
{
    public Task<ProfileStatus> GetProfileStatusByID(HttpContext context, Guid id);
    public Task UpdateProfile (HttpContext context, UserProfileUpdateDto dto);
}