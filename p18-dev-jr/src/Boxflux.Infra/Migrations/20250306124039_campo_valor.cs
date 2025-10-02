using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boxflux.Infra.Migrations
{
    /// <inheritdoc />
    public partial class campo_valor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Lauchings",
                type: "decimal(65,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "ConslidatedDaylies",
                type: "decimal(65,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "Lauchings",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,2)");

            migrationBuilder.AlterColumn<double>(
                name: "Balance",
                table: "ConslidatedDaylies",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,2)");
        }
    }
}
