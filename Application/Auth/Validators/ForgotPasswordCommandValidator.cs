using Application.Auth.Commands;
using FluentValidation;

namespace Application.Auth.Validators;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
                .WithMessage("Request body is required.")
            .SetValidator(new ForgotPasswordRequestValidator());
    }
}
