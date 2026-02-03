using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class FixUserProfilesSharedPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Drop FK to AspNetUsers (if exists)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserProfiles_AspNetUsers_AppUserId')
    ALTER TABLE [UserProfiles] DROP CONSTRAINT [FK_UserProfiles_AspNetUsers_AppUserId];
");

            // 2) Drop unique index on AppUserId (if exists)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserProfiles_AppUserId' AND object_id = OBJECT_ID('[UserProfiles]'))
    DROP INDEX [IX_UserProfiles_AppUserId] ON [UserProfiles];
");

            // 3) Drop PK (whatever it is currently)
            migrationBuilder.Sql(@"
DECLARE @pkName nvarchar(128);
SELECT @pkName = kc.name
FROM sys.key_constraints kc
WHERE kc.[type] = 'PK' AND kc.parent_object_id = OBJECT_ID('[UserProfiles]');

IF @pkName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [UserProfiles] DROP CONSTRAINT [' + @pkName + ']');
END
");

            // 4) Drop old Id column if it exists
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'Id' AND Object_ID = Object_ID(N'[UserProfiles]')
)
BEGIN
    ALTER TABLE [UserProfiles] DROP COLUMN [Id];
END
");

            // 5) Make AppUserId the PK
            migrationBuilder.Sql(@"
ALTER TABLE [UserProfiles]
ADD CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([AppUserId]);
");

            // 6) Re-add FK (AppUserId -> AspNetUsers.Id)
            migrationBuilder.Sql(@"
ALTER TABLE [UserProfiles]
ADD CONSTRAINT [FK_UserProfiles_AspNetUsers_AppUserId]
FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE;
");

            // 7) Ensure IsPublic default is true (optional, but matches your intent)
            migrationBuilder.Sql(@"
ALTER TABLE [UserProfiles]
ADD CONSTRAINT [DF_UserProfiles_IsPublic] DEFAULT (1) FOR [IsPublic];
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse operations (best-effort)

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserProfiles_AspNetUsers_AppUserId')
    ALTER TABLE [UserProfiles] DROP CONSTRAINT [FK_UserProfiles_AspNetUsers_AppUserId];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'PK_UserProfiles' AND parent_object_id = OBJECT_ID('[UserProfiles]'))
    ALTER TABLE [UserProfiles] DROP CONSTRAINT [PK_UserProfiles];
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'Id' AND Object_ID = Object_ID(N'[UserProfiles]')
)
BEGIN
    -- Id already exists, do nothing
END
ELSE
BEGIN
    ALTER TABLE [UserProfiles] ADD [Id] uniqueidentifier NOT NULL DEFAULT NEWID();
END
");

            migrationBuilder.Sql(@"
ALTER TABLE [UserProfiles]
ADD CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([Id]);
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserProfiles_AppUserId' AND object_id = OBJECT_ID('[UserProfiles]'))
    -- already exists
    SELECT 1;
ELSE
    CREATE UNIQUE INDEX [IX_UserProfiles_AppUserId] ON [UserProfiles]([AppUserId]);
");

            migrationBuilder.Sql(@"
ALTER TABLE [UserProfiles]
ADD CONSTRAINT [FK_UserProfiles_AspNetUsers_AppUserId]
FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_UserProfiles_IsPublic' AND parent_object_id = OBJECT_ID('[UserProfiles]'))
    ALTER TABLE [UserProfiles] DROP CONSTRAINT [DF_UserProfiles_IsPublic];
");
        }
    }
}
