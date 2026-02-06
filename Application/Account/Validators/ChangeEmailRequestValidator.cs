using Application.Account.Dtos;
using FluentValidation;

namespace Application.Account.Validators;

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("A valid email address is required.")
            .MaximumLength(256)
                .WithMessage("Email must not exceed 256 characters.");
    }
}
