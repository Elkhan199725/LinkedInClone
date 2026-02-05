using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class PostMedia : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public PostMediaType Type { get; set; }
    public string Url { get; set; } = null!;
    public string? PublicId { get; set; }
    public int Order { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; }
}
