using Application.Posts.Dtos;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
                .WithMessage("Comment text is required.")
            .MaximumLength(1500)
                .WithMessage("Comment text must not exceed 1500 characters.");
    }
}
