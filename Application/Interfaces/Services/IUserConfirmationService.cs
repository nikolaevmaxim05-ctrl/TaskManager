using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Application.DTOs.UserDTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IUserConfirmationService
{
    public Task SendConfirmationEmail(UserConfirmationsCreateDto dto);
}