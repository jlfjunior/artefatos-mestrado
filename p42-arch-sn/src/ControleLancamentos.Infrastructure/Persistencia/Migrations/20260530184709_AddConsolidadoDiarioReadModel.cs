using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleLancamentos.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddConsolidadoDiarioReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consolidados_diarios",
                columns: table => new
                {
                    data_referencia = table.Column<DateOnly>(type: "date", nullable: false),
                    total_creditos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_debitos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_lancamentos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consolidados_diarios", x => x.data_referencia);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consolidados_diarios");
        }
    }
}
