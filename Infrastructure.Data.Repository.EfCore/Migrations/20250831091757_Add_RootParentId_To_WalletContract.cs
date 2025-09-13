using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_RootParentId_To_WalletContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChangeStatusDate",
                schema: "Fc",
                table: "WalletContract",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RootParentId",
                schema: "Fc",
                table: "WalletContract",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_ParentId",
                schema: "Fc",
                table: "WalletContract",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_RootParentId",
                schema: "Fc",
                table: "WalletContract",
                column: "RootParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletContract_WalletContract_ParentId",
                schema: "Fc",
                table: "WalletContract",
                column: "ParentId",
                principalSchema: "Fc",
                principalTable: "WalletContract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletContract_WalletContract_RootParentId",
                schema: "Fc",
                table: "WalletContract",
                column: "RootParentId",
                principalSchema: "Fc",
                principalTable: "WalletContract",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletContract_WalletContract_ParentId",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletContract_WalletContract_RootParentId",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropIndex(
                name: "IX_WalletContract_ParentId",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropIndex(
                name: "IX_WalletContract_RootParentId",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropColumn(
                name: "ChangeStatusDate",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropColumn(
                name: "RootParentId",
                schema: "Fc",
                table: "WalletContract");
        }
    }
}
