using Application.Auth.Dtos;
using FluentValidation;

namespace Application.Auth.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
                .WithMessage("Reset code is required.")
            .Length(6)
                .WithMessage("Reset code must be exactly 6 digits.")
            .Matches(@"^\d{6}$")
                .WithMessage("Reset code must contain only digits.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
                .WithMessage("New password is required.")
            .MinimumLength(8)
                .WithMessage("New password must be at least 8 characters.");
    }
}
