using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminDatase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 1, 4 });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99d9e5b0-548c-42e6-bb96-c26b2bdf6e98", new DateTime(2026, 1, 17, 19, 18, 56, 673, DateTimeKind.Utc).AddTicks(9363), "AQAAAAIAAYagAAAAECdY4m9epYdWtUx5Fvz4V5XnVNgIr4AegidoN96uJVQbJ4lv+cMP86bBN/sTYzRLzg==", "872776ab-ebb7-47d7-afa1-4ff30e68d15c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 4 });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6a019505-ba5b-49ed-ade2-261143e57bd7", new DateTime(2026, 1, 17, 19, 16, 39, 687, DateTimeKind.Utc).AddTicks(5991), "AQAAAAIAAYagAAAAEA9srgqqCq7OY82916NEXNEIizK4LUEgvFhtFIup8/u0NWT8cqF/Fwwjikwg0/svew==", "d8e295b7-bad3-4423-9fcd-215025f1294e" });
        }
    }
}
