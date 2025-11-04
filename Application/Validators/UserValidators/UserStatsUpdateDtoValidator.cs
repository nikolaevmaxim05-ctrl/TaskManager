using FluentValidation;
using TaskManager.Application.DTOs.UserDTOs;

namespace TaskManager.Application.Validators;

public class UserStatsUpdateDtoValidator : AbstractValidator<UserProfileUpdateDto>
{
    public UserStatsUpdateDtoValidator()
    {
        // Валидируем никнейм
        RuleFor(x => x.NickName)
            .NotEmpty().WithMessage("Никнейм не может быть пустым.")
            .MinimumLength(3).WithMessage("Никнейм должен содержать минимум 3 символа.")
            .MaximumLength(32).WithMessage("Никнейм должен содержать не более 32 символов.")
            .Matches(@"^[a-zA-Z0-9а-яА-Я_\-]+$").WithMessage("Никнейм может содержать только буквы, цифры, дефис и нижнее подчёркивание.");

        // Валидируем файл (если он передан)
        When(x => x.Avatar != null, () =>
        {
            RuleFor(x => x.Avatar)
                .Must(f => f.Length <= 2 * 1024 * 1024)
                .WithMessage("Размер файла не должен превышать 2 МБ.")
                .Must(f => IsSupportedImage(f))
                .WithMessage("Разрешены только изображения форматов JPG, JPEG, PNG, GIF, WEBP.");
        });
    }

    private bool IsSupportedImage(IFormFile file)
    {
        if (file == null) return false;

        var allowedTypes = new[] {"image/jpg" ,"image/jpeg", "image/png", "image/gif", "image/webp" };
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        return allowedTypes.Contains(file.ContentType.ToLowerInvariant()) &&
               allowedExtensions.Contains(ext);
    }
}