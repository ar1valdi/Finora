using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finora.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBankingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                schema: "Finora",
                table: "User",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "Finora",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BankAccount",
                schema: "Finora",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankTransaction",
                schema: "Finora",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromBankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToBankAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    BankAccountId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankTransaction_BankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "Finora",
                        principalTable: "BankAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankTransaction_BankAccount_BankAccountId1",
                        column: x => x.BankAccountId1,
                        principalSchema: "Finora",
                        principalTable: "BankAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankTransaction_BankAccount_FromBankAccountId",
                        column: x => x.FromBankAccountId,
                        principalSchema: "Finora",
                        principalTable: "BankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankTransaction_BankAccount_ToBankAccountId",
                        column: x => x.ToBankAccountId,
                        principalSchema: "Finora",
                        principalTable: "BankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_BankAccountId",
                schema: "Finora",
                table: "User",
                column: "BankAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_BankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_BankAccountId1",
                schema: "Finora",
                table: "BankTransaction",
                column: "BankAccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_FromBankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                column: "FromBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_ToBankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                column: "ToBankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_BankAccount_BankAccountId",
                schema: "Finora",
                table: "User",
                column: "BankAccountId",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_BankAccount_BankAccountId",
                schema: "Finora",
                table: "User");

            migrationBuilder.DropTable(
                name: "BankTransaction",
                schema: "Finora");

            migrationBuilder.DropTable(
                name: "BankAccount",
                schema: "Finora");

            migrationBuilder.DropIndex(
                name: "IX_User_BankAccountId",
                schema: "Finora",
                table: "User");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "Finora",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "Finora",
                table: "User");
        }
    }
}
