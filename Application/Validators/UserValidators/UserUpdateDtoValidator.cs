using FluentValidation;
using TaskManager.Application.DTOs.UserDTOs;

namespace TaskManager.Application.Validators;

public class UserUpdateDtoValidator : AbstractValidator<UserAuthParamsUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        
        RuleFor(x => x.EMail)
            .NotEmpty().WithMessage("Email обязателен")
            .Matches(@"^[\w\.\-]+@[a-zA-Z\d\-]+(\.[a-zA-Z\d\-]+)*\.[a-zA-Z]{2,}$")
            .WithMessage("Некорректный формат email");

        RuleFor(x => x.NewPassword)
             .NotEmpty().WithMessage("Пароль обязателен")
             .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'"",.<>/?\\|`~]).{8,}$")
             .WithMessage("Пароль должен содержать минимум 8 символов, включая строчные, заглавные, цифры и спецсимволы");
         RuleFor(x => x.OldPassword)
           .NotEmpty().WithMessage("Пароль обязателен")
           .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'"",.<>/?\\|`~]).{8,}$")
           .WithMessage("Пароль должен содержать минимум 8 символов, включая строчные, заглавные, цифры и спецсимволы");
    }
}