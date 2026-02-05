using Application.Posts.Commands;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
                .WithMessage("Post ID is required.");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
                .WithMessage("Author ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
                .WithMessage("Request body is required.")
            .SetValidator(new AddCommentRequestValidator());
    }
}
