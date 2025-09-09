using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finora.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOutboxModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReplyTo",
                schema: "Finora",
                table: "OutboxMessage",
                newName: "RoutingKey");

            migrationBuilder.AddColumn<string>(
                name: "Exchange",
                schema: "Finora",
                table: "OutboxMessage",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exchange",
                schema: "Finora",
                table: "OutboxMessage");

            migrationBuilder.RenameColumn(
                name: "RoutingKey",
                schema: "Finora",
                table: "OutboxMessage",
                newName: "ReplyTo");
        }
    }
}
