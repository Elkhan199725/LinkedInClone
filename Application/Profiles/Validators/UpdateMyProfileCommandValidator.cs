using Application.Profiles.Commands;
using FluentValidation;

namespace Application.Profiles.Validators;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.AppUserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required.")
            .SetValidator(new UpdateMyProfileRequestValidator());
    }
}
