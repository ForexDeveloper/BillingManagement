using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Percentage_From_WalletContractGuarantor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Percentage",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "Percentage",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "Percentage",
                schema: "Fc",
                table: "WalletContractFacilitator");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(6,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(6,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(6,3)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
