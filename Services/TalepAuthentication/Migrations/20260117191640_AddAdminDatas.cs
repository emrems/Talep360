using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminDatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAtUtc", "DeletedAtUtc", "Email", "EmailConfirmed", "FullName", "IsActive", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TenantId", "TwoFactorEnabled", "UserName" },
                values: new object[] { 4, 0, "6a019505-ba5b-49ed-ade2-261143e57bd7", new DateTime(2026, 1, 17, 19, 16, 39, 687, DateTimeKind.Utc).AddTicks(5991), null, "admin@talep360.com", true, "System Admin", true, false, false, null, "ADMIN@TALEP360.COM", "ADMIN@TALEP360.COM", "AQAAAAIAAYagAAAAEA9srgqqCq7OY82916NEXNEIizK4LUEgvFhtFIup8/u0NWT8cqF/Fwwjikwg0/svew==", null, false, "d8e295b7-bad3-4423-9fcd-215025f1294e", 1, false, "admin@talep360.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAtUtc", "DeletedAtUtc", "Email", "EmailConfirmed", "FullName", "IsActive", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TenantId", "TwoFactorEnabled", "UserName" },
                values: new object[] { 1, 0, "d9355d98-e65a-47ff-bb79-abc773146871", new DateTime(2026, 1, 17, 19, 15, 15, 618, DateTimeKind.Utc).AddTicks(1773), null, "admin@talep360.com", true, "System Admin", true, false, false, null, "ADMIN@TALEP360.COM", "ADMIN@TALEP360.COM", "AQAAAAIAAYagAAAAEB1zb8/SmEmB/onWiHNxNRzDpAXNQ6j9OIp4KPY9kE0qDxDUG8nleFZBwemVqm7vlQ==", null, false, "2f50079c-ab7a-454d-81ae-3c8f28f95d8c", 1, false, "admin@talep360.com" });
        }
    }
}
