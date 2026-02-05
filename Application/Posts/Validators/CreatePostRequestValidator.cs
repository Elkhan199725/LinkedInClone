using Application.Posts.Dtos;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Text)
            .MaximumLength(3000)
                .WithMessage("Post text must not exceed 3000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Text));

        RuleFor(x => x.Visibility)
            .IsInEnum()
                .WithMessage("Invalid visibility value.");
    }
}
