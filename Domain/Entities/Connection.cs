using Domain.Common;

namespace Domain.Entities;

public class Connection : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid ConnectedUserId { get; set; }
    public AppUser ConnectedUser { get; set; } = null!;
}
