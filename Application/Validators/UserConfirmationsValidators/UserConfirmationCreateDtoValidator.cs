using FluentValidation;
using TaskManager.Application.DTOs.UserConfirmationsDTOs;

namespace TaskManager.Application.Validators;

public class UserConfirmationCreateDtoValidator : AbstractValidator<UserConfirmationsCreateDto>
{
    public UserConfirmationCreateDtoValidator()
    {
        RuleFor(x => x.EMail)
            .NotEmpty().WithMessage("Email обязателен")
            .Matches(@"^[\w\.\-]+@[a-zA-Z\d\-]+(\.[a-zA-Z\d\-]+)*\.[a-zA-Z]{2,}$")
            .WithMessage("Некорректный формат email");
    }
}