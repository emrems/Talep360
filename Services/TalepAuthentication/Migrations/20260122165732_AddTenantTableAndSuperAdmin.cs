using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantTableAndSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { 99, null, "SuperAdmin", "SUPERADMIN" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "20f1e254-7b50-416d-b1fe-75d0a724c390", new DateTime(2026, 1, 22, 16, 57, 30, 860, DateTimeKind.Utc).AddTicks(5941), "AQAAAAIAAYagAAAAEEIWszOD9EWgDDWMZcGXrNM2QvTiUFx/rNu3S0gZ6LJnqoziiVxl6jbrMt9GuAPY2w==", "fca086ef-cdf7-47e1-bf42-d2f2af4eeb37" });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAtUtc", "IsActive", "Name" },
                values: new object[] { 1, new DateTime(2026, 1, 22, 16, 57, 30, 860, DateTimeKind.Utc).AddTicks(5900), true, "Talep360 Host" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "535176a5-9045-424a-9a64-27210cade00c", new DateTime(2026, 1, 20, 12, 18, 22, 495, DateTimeKind.Utc).AddTicks(191), "AQAAAAIAAYagAAAAEGyLhMbtGnzL3cGM1gILGoX8FFrVpqHCpxOxBqBHodh4eGrx/pqw9hmaHXzIXfw3lA==", "92c9dc2e-cfa2-43cb-ad82-208a6a32ec3a" });
        }
    }
}
