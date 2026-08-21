using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleLancamentos.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lancamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_lancamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lancamentos", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lancamentos");
        }
    }
}
