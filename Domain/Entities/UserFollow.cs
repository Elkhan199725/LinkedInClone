using Domain.Common;

namespace Domain.Entities;

public class UserFollow : BaseEntity
{
    public Guid FollowerId { get; set; }
    public AppUser Follower { get; set; } = null!;

    public Guid FollowedUserId { get; set; }
    public AppUser FollowedUser { get; set; } = null!;
}
