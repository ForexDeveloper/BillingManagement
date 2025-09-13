using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Remove_MerchantContractId_From_FinancialDocumentPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialDocumentPayment_TenantMerchantContract_MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment");

            migrationBuilder.DropIndex(
                name: "IX_FinancialDocumentPayment_MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment");

            migrationBuilder.DropColumn(
                name: "MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "MerchantContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialDocumentPayment_TenantMerchantContract_MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "MerchantContractId",
                principalSchema: "Fc",
                principalTable: "TenantMerchantContract",
                principalColumn: "Id");
        }
    }
}
