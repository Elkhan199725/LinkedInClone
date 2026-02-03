using Application.Auth.Commands;
using FluentValidation;

namespace Application.Auth.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required.")
            .SetValidator(new LoginRequestValidator());
    }
}
