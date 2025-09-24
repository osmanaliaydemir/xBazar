using Application.DTOs.ApiKey;
using FluentValidation;

namespace Application.Validators;

public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .Matches("^[a-zA-Z0-9_\\s-]+$").WithMessage("Name can only contain letters, numbers, underscores, spaces, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.Environment)
            .MaximumLength(50).WithMessage("Environment cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Environment can only contain letters, numbers, underscores, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.Environment));
    }
}
