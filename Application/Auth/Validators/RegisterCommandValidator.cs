using Application.Auth.Commands;
using FluentValidation;

namespace Application.Auth.Validators;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required.")
            .SetValidator(new RegisterRequestValidator());
    }
}
