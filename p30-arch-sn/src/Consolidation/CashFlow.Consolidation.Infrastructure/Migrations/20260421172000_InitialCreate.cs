using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashFlow.Consolidation.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_consolidations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TotalCredits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalDebits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_consolidations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "processed_launch_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LaunchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_launch_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_consolidations_Date",
                table: "daily_consolidations",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_launch_events_LaunchId",
                table: "processed_launch_events",
                column: "LaunchId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_consolidations");

            migrationBuilder.DropTable(
                name: "processed_launch_events");
        }
    }
}
