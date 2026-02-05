using Application.Posts.Dtos;
using FluentValidation;

namespace Application.Posts.Validators;

public sealed class AddPostMediaRequestValidator : AbstractValidator<AddPostMediaRequest>
{
    private const string CloudinaryDomain = "https://res.cloudinary.com/";
    private const int MaxUrlLength = 500;

    public AddPostMediaRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
                .WithMessage("Invalid media type.");

        RuleFor(x => x.Url)
            .NotEmpty()
                .WithMessage("Media URL is required.")
            .MaximumLength(MaxUrlLength)
                .WithMessage($"Media URL must not exceed {MaxUrlLength} characters.")
            .Must(BeAValidCloudinaryUrl)
                .WithMessage($"Media URL must be a valid HTTPS URL from Cloudinary ({CloudinaryDomain}).");

        RuleFor(x => x.PublicId)
            .MaximumLength(255)
                .WithMessage("Public ID must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.PublicId));

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Order must be a non-negative number.");

        RuleFor(x => x.Width)
            .GreaterThan(0)
                .WithMessage("Width must be positive.")
            .When(x => x.Width.HasValue);

        RuleFor(x => x.Height)
            .GreaterThan(0)
                .WithMessage("Height must be positive.")
            .When(x => x.Height.HasValue);

        RuleFor(x => x.Duration)
            .GreaterThan(0)
                .WithMessage("Duration must be positive.")
            .When(x => x.Duration.HasValue);
    }

    private static bool BeAValidCloudinaryUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        return url.StartsWith(CloudinaryDomain, StringComparison.OrdinalIgnoreCase);
    }
}
