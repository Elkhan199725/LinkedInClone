using Application.Posts.Commands;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class ReactToPostCommandValidator : AbstractValidator<ReactToPostCommand>
{
    public ReactToPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
                .WithMessage("Post ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
                .WithMessage("Invalid reaction type.");
    }
}
