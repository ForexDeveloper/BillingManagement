using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_SettelmentType_To_LoanWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "SettlementType",
                schema: "Fc",
                table: "LoanWallet",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SettlementType",
                schema: "Fc",
                table: "Installment",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SettlementType",
                schema: "Fc",
                table: "Billing",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettlementType",
                schema: "Fc",
                table: "LoanWallet");

            migrationBuilder.DropColumn(
                name: "SettlementType",
                schema: "Fc",
                table: "Installment");

            migrationBuilder.DropColumn(
                name: "SettlementType",
                schema: "Fc",
                table: "Billing");
        }
    }
}
