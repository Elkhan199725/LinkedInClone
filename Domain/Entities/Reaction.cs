using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents a user's reaction to a post.
/// Each user can have only one reaction per post.
/// </summary>
public class Reaction : BaseEntity
{
    /// <summary>
    /// The post being reacted to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Navigation property to the post.
    /// </summary>
    public Post Post { get; set; } = null!;

    /// <summary>
    /// The user who reacted (FK to AppUser.Id).
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Type of reaction.
    /// </summary>
    public ReactionType Type { get; set; }
}
