using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalepAuthentication.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp", "Title" },
                values: new object[] { "5fe59eda-32d0-461c-b93f-967894fe8244", new DateTime(2026, 1, 22, 20, 53, 55, 83, DateTimeKind.Utc).AddTicks(5995), "AQAAAAIAAYagAAAAENXATZONQi7JRKRlddo8M4g1osOtC9GhG12NJJufomU6tT3v+HVyRXkGtgo80j1Nrw==", "6d30cb7e-9203-4bd5-866f-25876f2d3d25", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp", "Title" },
                values: new object[] { "59b54b5c-ee2b-417b-87ed-15842766e0e3", new DateTime(2026, 1, 22, 20, 53, 55, 153, DateTimeKind.Utc).AddTicks(4936), "AQAAAAIAAYagAAAAEJ/VdRroR2TvIPYhIslpiAxE+i+GFLCFsvNWQbgRY0xwOi5MB/1QtCQGLP6K3Ialwg==", "02504ebc-fda3-4489-9cfa-9cd8da9db551", null });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 1, 22, 20, 53, 55, 83, DateTimeKind.Utc).AddTicks(5926));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04bcd105-3db2-46a8-b23c-bf91c4194961", new DateTime(2026, 1, 22, 18, 8, 6, 573, DateTimeKind.Utc).AddTicks(4770), "AQAAAAIAAYagAAAAEBvU6ouUbbgDF0C/DKPwRLndhNgcSWZ7Oqn91J8Fao2p4P9F7s985cDHmewa6dBmSg==", "a3386823-2d6a-41fb-8d06-063d31bcab30" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "ConcurrencyStamp", "CreatedAtUtc", "PasswordHash", "SecurityStamp" },
                values: new object[] { "507d6d89-5632-45de-b564-254d41374df1", new DateTime(2026, 1, 22, 18, 8, 6, 639, DateTimeKind.Utc).AddTicks(3084), "AQAAAAIAAYagAAAAEF0QINkHWwr+YJSaqPxWK+sG/QfRFe1SVSPrTigE257s5I72tAJ5EnS9U/Z1fYtcZw==", "85d7d350-c8f6-4d1a-8388-789f34ed5ebe" });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 1, 22, 18, 8, 6, 573, DateTimeKind.Utc).AddTicks(4713));
        }
    }
}
