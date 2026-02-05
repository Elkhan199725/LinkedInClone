using Application.Posts.Commands;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class AddPostMediaCommandValidator : AbstractValidator<AddPostMediaCommand>
{
    public AddPostMediaCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
                .WithMessage("Post ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.");

        RuleFor(x => x.MediaItems)
            .NotEmpty()
                .WithMessage("At least one media item is required.")
            .Must(items => items.Count <= 10)
                .WithMessage("Maximum 10 media items allowed per request.");

        RuleForEach(x => x.MediaItems)
            .SetValidator(new AddPostMediaRequestValidator());
    }
}
