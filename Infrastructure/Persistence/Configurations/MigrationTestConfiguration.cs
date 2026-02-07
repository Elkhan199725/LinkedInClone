using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for MigrationTest entity.
/// This is a test table to verify migrations work - can be removed later.
/// </summary>
public sealed class MigrationTestConfiguration : IEntityTypeConfiguration<MigrationTest>
{
    public void Configure(EntityTypeBuilder<MigrationTest> builder)
    {
        builder.ToTable("MigrationTests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
