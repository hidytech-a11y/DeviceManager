using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceManager.Migrations
{
    /// <inheritdoc />
    public partial class DeleteAtUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Technicians",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Technicians",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Technicians");
        }
    }
}
