using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceManager.Migrations
{
    /// <inheritdoc />
    public partial class ModelRebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Technicians",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Technicians");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
