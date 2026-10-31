using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAppointment.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentReadyBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadyBy",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ReadyBy",
                table: "Appointments",
                column: "ReadyBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_ReadyBy",
                table: "Appointments",
                column: "ReadyBy",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_ReadyBy",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ReadyBy",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReadyBy",
                table: "Appointments");
        }
    }
}
