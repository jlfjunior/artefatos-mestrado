using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyCash.Report.Command.Store.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_consolidations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date of the transaction"),
                    total_credit = table.Column<decimal>(type: "numeric", nullable: false, comment: "Total sum of Credits em the date"),
                    total_debit = table.Column<decimal>(type: "numeric", nullable: false, comment: "Total sum of Debits em the date"),
                    final_balance = table.Column<decimal>(type: "numeric", nullable: false, comment: "Final balance of the date"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when the transaction was created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_consolidations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date of the transaction"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Description of the transaction"),
                    type = table.Column<string>(type: "text", nullable: false, comment: "Type of the transaction"),
                    amount = table.Column<decimal>(type: "numeric", nullable: false, comment: "Amount of the transaction"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when the transaction was created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_consolidations");

            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
