using Application.Profiles.Dtos;
using FluentValidation;

namespace Application.Profiles.Validators;

public sealed class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    private const string CloudinaryDomain = "https://res.cloudinary.com/";
    private const int MaxImageUrlLength = 500;

    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(x => x.Headline)
            .MaximumLength(120).WithMessage("Headline must not exceed 120 characters.")
            .When(x => !string.IsNullOrEmpty(x.Headline));

        RuleFor(x => x.About)
            .MaximumLength(2000).WithMessage("About must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.About));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.ProfilePhotoUrl)
            .MaximumLength(MaxImageUrlLength)
                .WithMessage($"Profile photo URL must not exceed {MaxImageUrlLength} characters.")
            .Must(BeAValidCloudinaryUrl)
                .WithMessage($"Profile photo URL must be a valid HTTPS URL from Cloudinary ({CloudinaryDomain}).")
            .When(x => !string.IsNullOrEmpty(x.ProfilePhotoUrl));

        RuleFor(x => x.CoverPhotoUrl)
            .MaximumLength(MaxImageUrlLength)
                .WithMessage($"Cover photo URL must not exceed {MaxImageUrlLength} characters.")
            .Must(BeAValidCloudinaryUrl)
                .WithMessage($"Cover photo URL must be a valid HTTPS URL from Cloudinary ({CloudinaryDomain}).")
            .When(x => !string.IsNullOrEmpty(x.CoverPhotoUrl));
    }

    private static bool BeAValidCloudinaryUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        return url.StartsWith(CloudinaryDomain, StringComparison.OrdinalIgnoreCase);
    }
}
