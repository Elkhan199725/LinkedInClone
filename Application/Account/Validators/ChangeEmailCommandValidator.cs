using Application.Account.Commands;
using FluentValidation;

namespace Application.Account.Validators;

public sealed class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
                .WithMessage("Request body is required.")
            .SetValidator(new ChangeEmailRequestValidator());
    }
}
