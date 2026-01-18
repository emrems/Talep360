using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAtUtc", "DeletedAtUtc", "Email", "EmailConfirmed", "FullName", "IsActive", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TenantId", "TwoFactorEnabled", "UserName" },
                values: new object[] { 1, 0, "d9355d98-e65a-47ff-bb79-abc773146871", new DateTime(2026, 1, 17, 19, 15, 15, 618, DateTimeKind.Utc).AddTicks(1773), null, "admin@talep360.com", true, "System Admin", true, false, false, null, "ADMIN@TALEP360.COM", "ADMIN@TALEP360.COM", "AQAAAAIAAYagAAAAEB1zb8/SmEmB/onWiHNxNRzDpAXNQ6j9OIp4KPY9kE0qDxDUG8nleFZBwemVqm7vlQ==", null, false, "2f50079c-ab7a-454d-81ae-3c8f28f95d8c", 1, false, "admin@talep360.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
