using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash", "TimeZone" },
                values: new object[,]
                {
                    { new Guid("a11ce000-0000-7000-8000-000000000001"), "demo@example.test", "Alice Martins", "AQAAAAIAAYagAAAAECuoL6m7HNzz0v7D5ihyiEnloVLgSf0Q11bu2OECMqfsdKZybfgw4KUYjkAGF2DEpw==", "America/Sao_Paulo" },
                    { new Guid("b0b00000-0000-7000-8000-000000000002"), "bob@example.test", "Bob Tanaka", "AQAAAAIAAYagAAAAECuoL6m7HNzz0v7D5ihyiEnloVLgSf0Q11bu2OECMqfsdKZybfgw4KUYjkAGF2DEpw==", "Asia/Tokyo" }
                });

            migrationBuilder.InsertData(
                table: "TaskItems",
                columns: new[] { "Id", "Description", "DueDateUtc", "Status", "Title", "UserId" },
                values: new object[,]
                {
                    { new Guid("a11ce000-7a5c-7000-8000-000000000001"), "Review Clean Architecture", new DateTime(2026, 10, 21, 18, 0, 0, 0, DateTimeKind.Utc), "Todo", "Prepare technical interview", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000002"), "Intermittent timeout on CI.", new DateTime(2026, 10, 14, 13, 0, 0, 0, DateTimeKind.Utc), "InProgress", "Fix flaky login test", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000003"), "Describe the auth and tasks endpoints.", new DateTime(2026, 10, 18, 20, 0, 0, 0, DateTimeKind.Utc), "InProgress", "Write API documentation", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000004"), null, new DateTime(2026, 10, 10, 12, 0, 0, 0, DateTimeKind.Utc), "Todo", "Renew passport", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000005"), "September travel expenses.", new DateTime(2026, 10, 5, 21, 0, 0, 0, DateTimeKind.Utc), "Done", "Submit expense report", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000006"), null, new DateTime(2026, 11, 20, 17, 0, 0, 0, DateTimeKind.Utc), "InProgress", "Refactor task board styles", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000007"), null, new DateTime(2026, 12, 3, 12, 30, 0, 0, DateTimeKind.Utc), "Todo", "Book dentist appointment", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("a11ce000-7a5c-7000-8000-000000000008"), "Draft goals and milestones for next quarter.", new DateTime(2027, 1, 15, 13, 0, 0, 0, DateTimeKind.Utc), "Todo", "Plan Q1 roadmap", new Guid("a11ce000-0000-7000-8000-000000000001") },
                    { new Guid("b0b00000-7a5c-7000-8000-000000000001"), null, new DateTime(2026, 10, 20, 1, 0, 0, 0, DateTimeKind.Utc), "InProgress", "Review pull requests", new Guid("b0b00000-0000-7000-8000-000000000002") },
                    { new Guid("b0b00000-7a5c-7000-8000-000000000002"), "Slides and a short live demo.", new DateTime(2026, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Todo", "Prepare sprint demo", new Guid("b0b00000-0000-7000-8000-000000000002") },
                    { new Guid("b0b00000-7a5c-7000-8000-000000000003"), null, new DateTime(2026, 10, 15, 8, 0, 0, 0, DateTimeKind.Utc), "Done", "Update team wiki", new Guid("b0b00000-0000-7000-8000-000000000002") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-7a5c-7000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("b0b00000-7a5c-7000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("b0b00000-7a5c-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TaskItems",
                keyColumn: "Id",
                keyValue: new Guid("b0b00000-7a5c-7000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a11ce000-0000-7000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b0b00000-0000-7000-8000-000000000002"));
        }
    }
}
