using FluentValidation;

namespace TaskManager.Application.Validators;

public class ImageFileValidator : AbstractValidator<IFormFile>
{
    public ImageFileValidator()
    {
        // Проверяем тип
        RuleFor(f => f.ContentType)
            .Must(type => type == "image/jpeg" ||
                          type == "image/png" ||
                          type == "image/webp" ||
                          type == "image/jpg")
            .WithMessage("Допустимые форматы изображений: JPEG, PNG, WEBP.");

        // Проверяем размер
        RuleFor(f => f.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024) // 5 MB
            .WithMessage("Размер каждого изображения не должен превышать 5 МБ.");
        
        RuleFor(f => f)
            .NotNull()
            .WithMessage("Изображение не может быть null");
    }
}