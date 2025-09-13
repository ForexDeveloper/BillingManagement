using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_MerchantBranchId_To_FinancialDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Fc",
                table: "Plan",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MerchantBranchId",
                schema: "Fc",
                table: "FinancialDocument",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantMerchantContractId",
                schema: "Fc",
                table: "FinancialDocument",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantPlatformContractId",
                schema: "Fc",
                table: "FinancialDocument",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_MerchantBranchId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "MerchantBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_TenantMerchantContractId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "TenantMerchantContractId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_TenantPlatformContractId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "TenantPlatformContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialDocument_MerchantBranch_MerchantBranchId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "MerchantBranchId",
                principalSchema: "Fc",
                principalTable: "MerchantBranch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialDocument_TenantMerchantContract_TenantMerchantContractId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "TenantMerchantContractId",
                principalSchema: "Fc",
                principalTable: "TenantMerchantContract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialDocument_TenantPlatformContract_TenantPlatformContractId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "TenantPlatformContractId",
                principalSchema: "Fc",
                principalTable: "TenantPlatformContract",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialDocument_MerchantBranch_MerchantBranchId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialDocument_TenantMerchantContract_TenantMerchantContractId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialDocument_TenantPlatformContract_TenantPlatformContractId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropIndex(
                name: "IX_FinancialDocument_MerchantBranchId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropIndex(
                name: "IX_FinancialDocument_TenantMerchantContractId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropIndex(
                name: "IX_FinancialDocument_TenantPlatformContractId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Fc",
                table: "Plan");

            migrationBuilder.DropColumn(
                name: "MerchantBranchId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "TenantMerchantContractId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "TenantPlatformContractId",
                schema: "Fc",
                table: "FinancialDocument");
        }
    }
}
