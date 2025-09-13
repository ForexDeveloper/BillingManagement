using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Change_WalletContract_PortionType_To_PortionTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractGuarantor",
                newName: "PortionTypes");

            migrationBuilder.RenameColumn(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractFinancier",
                newName: "PortionTypes");

            migrationBuilder.RenameColumn(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractFacilitator",
                newName: "PortionTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PortionTypes",
                schema: "Fc",
                table: "WalletContractGuarantor",
                newName: "PortionType");

            migrationBuilder.RenameColumn(
                name: "PortionTypes",
                schema: "Fc",
                table: "WalletContractFinancier",
                newName: "PortionType");

            migrationBuilder.RenameColumn(
                name: "PortionTypes",
                schema: "Fc",
                table: "WalletContractFacilitator",
                newName: "PortionType");
        }
    }
}
