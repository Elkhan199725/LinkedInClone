using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Simple test entity to verify EF Core migrations work correctly.
/// Can be safely removed once migration pipeline is confirmed working.
/// </summary>
public class MigrationTest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
