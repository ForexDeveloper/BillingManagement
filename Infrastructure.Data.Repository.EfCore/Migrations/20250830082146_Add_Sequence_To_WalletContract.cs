using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sequence_To_WalletContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.CreateSequence(
                name: "WalletContractNumber",
                schema: "Fc",
                startValue: 300L);

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                schema: "Fc",
                table: "WalletContract",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                defaultValueSql: "'c-' + CAST(NEXT VALUE FOR Fc.WalletContractNumber AS NVARCHAR(30))");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "Fc",
                table: "WalletContract",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "Fc",
                table: "WalletContract",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractNumber",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "Fc",
                table: "WalletContract");

            migrationBuilder.DropSequence(
                name: "WalletContractNumber",
                schema: "Fc");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "Fc",
                table: "WalletContract",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
