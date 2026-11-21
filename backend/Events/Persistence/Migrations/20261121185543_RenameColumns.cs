using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DispayName",
                table: "AspNetUsers",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "isHost",
                table: "ActivityAttendees",
                newName: "IsHost");

            migrationBuilder.RenameColumn(
                name: "isCancelled",
                table: "Activities",
                newName: "IsCancelled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "AspNetUsers",
                newName: "DispayName");

            migrationBuilder.RenameColumn(
                name: "IsHost",
                table: "ActivityAttendees",
                newName: "isHost");

            migrationBuilder.RenameColumn(
                name: "IsCancelled",
                table: "Activities",
                newName: "isCancelled");
        }
    }
}
