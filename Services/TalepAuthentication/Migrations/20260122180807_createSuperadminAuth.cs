using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class createSuperadminAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04bcd105-3db2-46a8-b23c-bf91c4194961", new DateTime(2026, 1, 22, 18, 8, 6, 573, DateTimeKind.Utc).AddTicks(4770), "AQAAAAIAAYagAAAAEBvU6ouUbbgDF0C/DKPwRLndhNgcSWZ7Oqn91J8Fao2p4P9F7s985cDHmewa6dBmSg==", "a3386823-2d6a-41fb-8d06-063d31bcab30" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAtUtc", "DeletedAtUtc", "Email", "EmailConfirmed", "FullName", "IsActive", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TenantId", "TwoFactorEnabled", "UserName" },
                values: new object[] { 99, 0, "507d6d89-5632-45de-b564-254d41374df1", new DateTime(2026, 1, 22, 18, 8, 6, 639, DateTimeKind.Utc).AddTicks(3084), null, "superadmin@talep360.com", true, "Super Administrator", true, false, false, null, "SUPERADMIN@TALEP360.COM", "SUPERADMIN@TALEP360.COM", "AQAAAAIAAYagAAAAEF0QINkHWwr+YJSaqPxWK+sG/QfRFe1SVSPrTigE257s5I72tAJ5EnS9U/Z1fYtcZw==", null, false, "85d7d350-c8f6-4d1a-8388-789f34ed5ebe", 1, false, "superadmin@talep360.com" });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 1, 22, 18, 8, 6, 573, DateTimeKind.Utc).AddTicks(4713));

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 99, 99 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 99, 99 });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "20f1e254-7b50-416d-b1fe-75d0a724c390", new DateTime(2026, 1, 22, 16, 57, 30, 860, DateTimeKind.Utc).AddTicks(5941), "AQAAAAIAAYagAAAAEEIWszOD9EWgDDWMZcGXrNM2QvTiUFx/rNu3S0gZ6LJnqoziiVxl6jbrMt9GuAPY2w==", "fca086ef-cdf7-47e1-bf42-d2f2af4eeb37" });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 1, 22, 16, 57, 30, 860, DateTimeKind.Utc).AddTicks(5900));
        }
    }
}
