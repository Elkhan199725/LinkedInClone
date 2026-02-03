using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillUserProfilesFromAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill UserProfiles from AspNetUsers for any users that don't have a profile yet
            // This is idempotent - only inserts rows where they don't already exist
            migrationBuilder.Sql(@"
                INSERT INTO [UserProfiles] (
                    [AppUserId],
                    [FirstName],
                    [LastName],
                    [Headline],
                    [About],
                    [Location],
                    [ProfilePhotoUrl],
                    [CoverPhotoUrl],
                    [IsPublic],
                    [CreatedAt],
                    [UpdatedAt]
                )
                SELECT 
                    u.[Id],
                    ISNULL(u.[FirstName], 'Unknown'),
                    ISNULL(u.[LastName], 'User'),
                    u.[Headline],
                    u.[About],
                    u.[Location],
                    u.[ProfilePhotoUrl],
                    u.[CoverPhotoUrl],
                    1,
                    GETUTCDATE(),
                    NULL
                FROM [AspNetUsers] u
                WHERE NOT EXISTS (
                    SELECT 1 FROM [UserProfiles] p WHERE p.[AppUserId] = u.[Id]
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // We don't delete the backfilled profiles on rollback
            // as that could cause data loss
        }
    }
}
