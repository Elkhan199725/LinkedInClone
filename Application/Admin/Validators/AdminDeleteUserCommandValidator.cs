using Application.Admin.Commands;
using FluentValidation;

namespace Application.Admin.Validators;

public sealed class AdminDeleteUserCommandValidator : AbstractValidator<AdminDeleteUserCommand>
{
    public AdminDeleteUserCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty()
                .WithMessage("Target user ID is required.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
                .WithMessage("Requesting user ID is required.");
    }
}
