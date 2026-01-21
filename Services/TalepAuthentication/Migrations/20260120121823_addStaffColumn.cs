using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class addStaffColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { 5, null, "Staff", "STAFF" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "535176a5-9045-424a-9a64-27210cade00c", new DateTime(2026, 1, 20, 12, 18, 22, 495, DateTimeKind.Utc).AddTicks(191), "AQAAAAIAAYagAAAAEGyLhMbtGnzL3cGM1gILGoX8FFrVpqHCpxOxBqBHodh4eGrx/pqw9hmaHXzIXfw3lA==", "92c9dc2e-cfa2-43cb-ad82-208a6a32ec3a" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99d9e5b0-548c-42e6-bb96-c26b2bdf6e98", new DateTime(2026, 1, 17, 19, 18, 56, 673, DateTimeKind.Utc).AddTicks(9363), "AQAAAAIAAYagAAAAECdY4m9epYdWtUx5Fvz4V5XnVNgIr4AegidoN96uJVQbJ4lv+cMP86bBN/sTYzRLzg==", "872776ab-ebb7-47d7-afa1-4ff30e68d15c" });
        }
    }
}
