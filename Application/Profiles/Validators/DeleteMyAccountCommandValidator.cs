using Application.Profiles.Commands;
using FluentValidation;

namespace Application.Profiles.Validators;

public sealed class DeleteMyAccountCommandValidator : AbstractValidator<DeleteMyAccountCommand>
{
    public DeleteMyAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");
    }
}
