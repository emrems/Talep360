using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepService.Migrations
{
    /// <inheritdoc />
    public partial class TicketWorkflowUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAssignmentApproved",
                table: "Tickets",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAssignmentApproved",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "Tickets");
        }
    }
}
