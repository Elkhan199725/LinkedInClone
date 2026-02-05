using Application.Posts.Commands;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.AuthorId)
            .NotEmpty()
                .WithMessage("Author ID is required.");

        RuleFor(x => x.Request)
            .NotNull()
                .WithMessage("Request body is required.")
            .SetValidator(new CreatePostRequestValidator());
    }
}
