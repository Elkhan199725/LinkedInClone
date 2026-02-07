using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.ToTable("UserFollows");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Follower)
            .WithMany()
            .HasForeignKey(x => x.FollowerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.FollowedUser)
            .WithMany()
            .HasForeignKey(x => x.FollowedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.FollowerId, x.FollowedUserId })
            .IsUnique();

        builder.HasIndex(x => x.FollowedUserId);
    }
}
