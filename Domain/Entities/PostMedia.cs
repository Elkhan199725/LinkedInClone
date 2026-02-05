using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents a media item (image or video) attached to a post.
/// </summary>
public class PostMedia : BaseEntity
{
    /// <summary>
    /// The post this media belongs to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Navigation property to the parent post.
    /// </summary>
    public Post Post { get; set; } = null!;

    /// <summary>
    /// Type of media (Image or Video).
    /// </summary>
    public PostMediaType Type { get; set; }

    /// <summary>
    /// Cloudinary URL of the media file.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Cloudinary public ID for potential future management.
    /// </summary>
    public string? PublicId { get; set; }

    /// <summary>
    /// Display order of media within the post.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Width of the media in pixels (for images/videos).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Height of the media in pixels (for images/videos).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Duration in seconds (for videos only).
    /// </summary>
    public int? Duration { get; set; }
}
