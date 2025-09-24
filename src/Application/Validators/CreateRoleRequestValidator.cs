using Application.DTOs.Role;
using FluentValidation;

namespace Application.Validators;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9_\\s]+$").WithMessage("Role name can only contain letters, numbers, underscores, and spaces");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.IsSystemRole)
            .NotNull().WithMessage("IsSystemRole must be specified");

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions must be specified")
            .Must(permissions => permissions != null && permissions.All(p => !string.IsNullOrWhiteSpace(p)))
            .WithMessage("All permissions must be valid")
            .When(x => x.Permissions != null && x.Permissions.Any());
    }
}
