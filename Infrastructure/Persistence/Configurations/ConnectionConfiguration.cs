using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("Connections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ConnectedUser)
            .WithMany()
            .HasForeignKey(x => x.ConnectedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.UserId, x.ConnectedUserId })
            .IsUnique();

        builder.HasIndex(x => x.ConnectedUserId);
    }
}
