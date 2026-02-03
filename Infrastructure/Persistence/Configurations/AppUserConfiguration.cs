using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Headline)
            .HasMaxLength(200);

        builder.Property(x => x.About)
            .HasMaxLength(2000);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.ProfilePhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.CoverPhotoUrl)
            .HasMaxLength(500);

        // Helpful indexes for common queries/sorts
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.Location);
    }
}
