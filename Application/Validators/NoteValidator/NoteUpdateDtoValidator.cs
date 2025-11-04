using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class NoteUpdateDtoValidator : AbstractValidator<NoteUpdateDto>
{
    public NoteUpdateDtoValidator()
    {
        RuleFor(x => x.HeadOfNote)
            .NotEmpty().WithMessage("Заголовок не может быть пустым.")
            .MaximumLength(200).WithMessage("Заголовок не должен превышать 200 символов.");

        RuleFor(x => x.BodyOfNote)
            .NotEmpty().WithMessage("Описание не может быть пустым.");

        RuleFor(x => x.DateOfDeadLine)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("Дата дедлайна должна быть в будущем.");

        // Проверяем, есть ли вообще изображения
        When(x => x.Images != null && x.Images.Any(), () =>
        {
            RuleForEach(x => x.Images).SetValidator(new ImageFileValidator());

            RuleFor(x => x.Images)
                .Must(list => list.Count <= 10)
                .WithMessage("Можно загрузить не более 10 изображений.");
        });
    }
}