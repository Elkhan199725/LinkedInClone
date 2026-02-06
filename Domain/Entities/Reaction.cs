using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Reaction : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public ReactionType Type { get; set; }
}
