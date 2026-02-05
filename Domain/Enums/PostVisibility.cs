namespace Domain.Enums;

/// <summary>
/// Defines who can see a post.
/// </summary>
public enum PostVisibility
{
    /// <summary>
    /// Visible to everyone.
    /// </summary>
    Public = 0,

    /// <summary>
    /// Visible only to the author's connections.
    /// </summary>
    Connections = 1,

    /// <summary>
    /// Visible only to the author.
    /// </summary>
    Private = 2
}
