using TaskManager.Application.DTOs.UserConfirmationsDTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Mappers;

public static class UserConfirmationsMapper
{
    public static UserConfirmation ToDomain(this UserConfirmationsCreateDto dto, string token)
        => new ()
        {
            Id = Guid.NewGuid(),
            Email = dto.EMail,
            Expiration = DateTime.Now + TimeSpan.FromMinutes(1),
            Token = token
        };
}