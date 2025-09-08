using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finora.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_BankAccount_BankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_BankAccount_BankAccountId1",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_BankAccount_FromBankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_BankAccount_ToBankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_BankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_BankAccountId1",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_FromBankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_ToBankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropColumn(
                name: "BankAccountId1",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropColumn(
                name: "FromBankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropColumn(
                name: "ToBankAccountId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_FromId",
                schema: "Finora",
                table: "BankTransaction",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransaction_ToId",
                schema: "Finora",
                table: "BankTransaction",
                column: "ToId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_BankAccount_FromId",
                schema: "Finora",
                table: "BankTransaction",
                column: "FromId",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_BankAccount_ToId",
                schema: "Finora",
                table: "BankTransaction",
                column: "ToId",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_BankAccount_FromId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_BankAccount_ToId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_FromId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BankTransaction_ToId",
                schema: "Finora",
                table: "BankTransaction");

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId1",
                schema: "Finora",
                table: "BankTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromBankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ToBankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                type: "uuid",
                nullable: true);

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
                name: "FK_BankTransaction_BankAccount_BankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                column: "BankAccountId",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_BankAccount_BankAccountId1",
                schema: "Finora",
                table: "BankTransaction",
                column: "BankAccountId1",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_BankAccount_FromBankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                column: "FromBankAccountId",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_BankAccount_ToBankAccountId",
                schema: "Finora",
                table: "BankTransaction",
                column: "ToBankAccountId",
                principalSchema: "Finora",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
