using FluentValidation;
using TaskManager.Application.DTOs.ChatDTOs;

namespace TaskManager.Application.Validators.MessageValidators;

public class MessageCreateDtoValidator : AbstractValidator<MessageCreateDto>
{
    public MessageCreateDtoValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Сообщение не может быть пустым")
            .MaximumLength(1000).WithMessage("Длинна сообщения не может превышать 1000 символов");
        RuleFor(x => x.Sender)
            .Must(x => x != Guid.Empty).WithMessage("Id отправителя не может быть пустым");
    }
}