using Application.Posts.Commands;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
                .WithMessage("Post ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
                .WithMessage("Request body is required.")
            .SetValidator(new UpdatePostRequestValidator());
    }
}
