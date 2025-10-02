using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyCash.Command.Store.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotent_messages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    request = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "Request of the message"),
                    response = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true, comment: "Response of the message")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotent_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Type of class for desserializing"),
                    content = table.Column<string>(type: "jsonb", nullable: false, comment: "Content of the message"),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when the message was processed"),
                    error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Error message if the message processing failed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
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
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Category of the transaction"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when the transaction was created"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when the transaction was updated"),
                    row_version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1", comment: "Concurrency Token")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "The real first name of the user"),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "The real last name of the user"),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "user e-mail"),
                    identity_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_idempotent_messages_id",
                table: "idempotent_messages",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on_utc_processed_on_utc",
                table: "outbox_messages",
                columns: new[] { "occurred_on_utc", "processed_on_utc" })
                .Annotation("Npgsql:IndexInclude", new[] { "id", "type", "content" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_date",
                table: "transactions",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_identity_id",
                table: "users",
                column: "identity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotent_messages");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
