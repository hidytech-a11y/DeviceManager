using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianDiagnosis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "Diagnoses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_TechnicianId",
                table: "Diagnoses",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnoses_Technicians_TechnicianId",
                table: "Diagnoses",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnoses_Technicians_TechnicianId",
                table: "Diagnoses");

            migrationBuilder.DropIndex(
                name: "IX_Diagnoses_TechnicianId",
                table: "Diagnoses");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "Diagnoses");
        }
    }
}
