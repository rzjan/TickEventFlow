using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Query.Infrastructure.Persistence.Migations
{
    /// <inheritdoc />
    public partial class RenameEmploteeIdToEmployeeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketEmployees_Employees_EmploteeId",
                table: "TicketEmployees");

            migrationBuilder.RenameColumn(
                name: "EmploteeId",
                table: "TicketEmployees",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketEmployees_EmploteeId",
                table: "TicketEmployees",
                newName: "IX_TicketEmployees_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEmployees_Employees_EmployeeId",
                table: "TicketEmployees",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketEmployees_Employees_EmployeeId",
                table: "TicketEmployees");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "TicketEmployees",
                newName: "EmploteeId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketEmployees_EmployeeId",
                table: "TicketEmployees",
                newName: "IX_TicketEmployees_EmploteeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketEmployees_Employees_EmploteeId",
                table: "TicketEmployees",
                column: "EmploteeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
