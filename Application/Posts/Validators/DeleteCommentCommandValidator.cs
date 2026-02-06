using Application.Posts.Commands;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
                .WithMessage("Post ID is required.");

        RuleFor(x => x.CommentId)
            .NotEmpty()
                .WithMessage("Comment ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");
    }
}
