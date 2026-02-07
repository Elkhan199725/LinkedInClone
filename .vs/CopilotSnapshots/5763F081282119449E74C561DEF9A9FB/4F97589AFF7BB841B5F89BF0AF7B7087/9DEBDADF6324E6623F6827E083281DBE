using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(x => x.AppUserId);

        builder.HasOne(x => x.AppUser)
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Headline)
            .HasMaxLength(120);

        builder.Property(x => x.About)
            .HasMaxLength(2000);

        builder.Property(x => x.Location)
            .HasMaxLength(100);

        builder.Property(x => x.ProfilePhotoUrl)
            .HasMaxLength(300);

        builder.Property(x => x.CoverPhotoUrl)
            .HasMaxLength(300);

        builder.Property(x => x.IsPublic)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}