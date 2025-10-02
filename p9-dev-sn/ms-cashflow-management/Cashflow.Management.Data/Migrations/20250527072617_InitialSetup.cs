using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Management.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashStatements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    Inflow = table.Column<decimal>(type: "numeric", nullable: false),
                    Outflow = table.Column<decimal>(type: "numeric", nullable: false),
                    isOpening = table.Column<bool>(type: "boolean", nullable: false),
                    OpeningDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashStatements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsolidatedCashBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedCashBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsolidatedTransactionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CashStatementId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedTransactionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidatedTransactionHistories_CashStatements_CashStateme~",
                        column: x => x.CashStatementId,
                        principalTable: "CashStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedTransactionHistories_CashStatementId",
                table: "ConsolidatedTransactionHistories",
                column: "CashStatementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsolidatedCashBalances");

            migrationBuilder.DropTable(
                name: "ConsolidatedTransactionHistories");

            migrationBuilder.DropTable(
                name: "CashStatements");
        }
    }
}
