using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserProfileSharedPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop the existing foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_AppUserId",
                table: "UserProfiles");

            // Step 2: Drop the existing unique index on AppUserId
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_AppUserId",
                table: "UserProfiles");

            // Step 3: Drop the existing primary key (Id)
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfiles",
                table: "UserProfiles");

            // Step 4: Drop the old Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserProfiles");

            // Step 5: Make AppUserId the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfiles",
                table: "UserProfiles",
                column: "AppUserId");

            // Step 6: Re-add the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_AppUserId",
                table: "UserProfiles",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Step 7: Add default value for IsPublic
            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_AppUserId",
                table: "UserProfiles");

            // Reverse: Drop new primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfiles",
                table: "UserProfiles");

            // Reverse: Add back the Id column
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            // Reverse: Re-add the old primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfiles",
                table: "UserProfiles",
                column: "Id");

            // Reverse: Re-add unique index on AppUserId
            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_AppUserId",
                table: "UserProfiles",
                column: "AppUserId",
                unique: true);

            // Reverse: Re-add foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_AppUserId",
                table: "UserProfiles",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Reverse: Remove default value for IsPublic
            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
