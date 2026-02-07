using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class ConnectionRequest : BaseEntity
{
    public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public AppUser Receiver { get; set; } = null!;

    public ConnectionRequestStatus Status { get; set; } = ConnectionRequestStatus.Pending;
    public DateTime? RespondedAt { get; set; }
}
