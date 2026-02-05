using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a comment on a post. Supports nested replies via ParentCommentId.
/// </summary>
public class Comment : BaseEntity
{
    /// <summary>
    /// The post this comment belongs to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Navigation property to the post.
    /// </summary>
    public Post Post { get; set; } = null!;

    /// <summary>
    /// The user who wrote this comment (FK to AppUser.Id).
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property to the author.
    /// </summary>
    public AppUser Author { get; set; } = null!;

    /// <summary>
    /// Parent comment ID for nested replies. Null for top-level comments.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation property to the parent comment.
    /// </summary>
    public Comment? ParentComment { get; set; }

    /// <summary>
    /// Replies to this comment.
    /// </summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    /// <summary>
    /// The text content of the comment.
    /// </summary>
    public string Text { get; set; } = null!;
}
