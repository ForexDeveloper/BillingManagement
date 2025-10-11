using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class modify_billing_installment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billing_Billing_BillingId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropForeignKey(
                name: "FK_Billing_Billing_ParentId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Billing_BillingId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_BusinessIdentity_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_BusinessIdentity_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_FinancialDocument_FinancialDocumentId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_TenantMerchantContract_TenantMerchantContractId1",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Tenant_TenantId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_MerchantBilling_Merchant_MerchantId",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropForeignKey(
                name: "FK_MerchantInstallment_Installments_Id",
                schema: "Bill",
                table: "MerchantInstallment");

            migrationBuilder.DropIndex(
                name: "IX_MerchantBilling_MerchantId",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropIndex(
                name: "IX_Billing_ParentId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Installments",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropIndex(
                name: "IX_Installments_BillingId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropIndex(
                name: "IX_Installments_TenantMerchantContractId1",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropColumn(
                name: "Additions",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropColumn(
                name: "AdditionsDescription",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropColumn(
                name: "CurrentPeriodCalculatedCommission",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropColumn(
                name: "DeductionsDescription",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                schema: "Bill",
                table: "MerchantBilling");

            migrationBuilder.DropColumn(
                name: "BillingId",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.DropColumn(
                name: "TenantMerchantContractId1",
                schema: "Bill",
                table: "Installments");

            migrationBuilder.RenameTable(
                name: "Installments",
                schema: "Bill",
                newName: "Installment",
                newSchema: "Bill");

            migrationBuilder.RenameColumn(
                name: "PreviousPeriodRefundedTransactions",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "RefundedTransactionsAmount");

            migrationBuilder.RenameColumn(
                name: "Deductions",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "PurchaseTransactionsCommission");

            migrationBuilder.RenameColumn(
                name: "CurrentPeriodPurchaseTransactions",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "PurchaseTransactionsCalculatedCommission");

            migrationBuilder.RenameColumn(
                name: "CurrentPeriodFinalCommission",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "PurchaseTransactionsAmount");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                schema: "Bill",
                table: "Billing",
                newName: "DebtorId");

            migrationBuilder.RenameColumn(
                name: "BillingId",
                schema: "Bill",
                table: "Billing",
                newName: "CreditorId");

            migrationBuilder.RenameIndex(
                name: "IX_Billing_BillingId",
                schema: "Bill",
                table: "Billing",
                newName: "IX_Billing_CreditorId");

            migrationBuilder.RenameIndex(
                name: "IX_Installments_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                newName: "IX_Installment_ToBusinessIdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Installments_TenantId",
                schema: "Bill",
                table: "Installment",
                newName: "IX_Installment_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Installments_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                newName: "IX_Installment_FromBusinessIdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Installments_FinancialDocumentId",
                schema: "Bill",
                table: "Installment",
                newName: "IX_Installment_FinancialDocumentId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Commission",
                schema: "Bill",
                table: "FinancialDocument",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContractIds",
                schema: "Bill",
                table: "Billing",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionsAmount",
                schema: "Bill",
                table: "Billing",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AdditionsDescription",
                schema: "Bill",
                table: "Billing",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeductionsAmount",
                schema: "Bill",
                table: "Billing",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeductionsDescription",
                schema: "Bill",
                table: "Billing",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MainContractId",
                schema: "Bill",
                table: "Billing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Transferred",
                schema: "Bill",
                table: "Billing",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CashAmount",
                schema: "Bill",
                table: "Installment",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditAmount",
                schema: "Bill",
                table: "Installment",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentAmount",
                schema: "Bill",
                table: "Installment",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Installment",
                schema: "Bill",
                table: "Installment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJob", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billing_DebtorId",
                schema: "Bill",
                table: "Billing",
                column: "DebtorId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_JobId",
                schema: "Bill",
                table: "BackgroundJob",
                column: "JobId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_Billing_CreditorId",
                schema: "Bill",
                table: "Billing",
                column: "CreditorId",
                principalSchema: "Bill",
                principalTable: "Billing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_Billing_DebtorId",
                schema: "Bill",
                table: "Billing",
                column: "DebtorId",
                principalSchema: "Bill",
                principalTable: "Billing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installment_BusinessIdentity_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                column: "FromBusinessIdentityId",
                principalSchema: "Bill",
                principalTable: "BusinessIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installment_BusinessIdentity_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                column: "ToBusinessIdentityId",
                principalSchema: "Bill",
                principalTable: "BusinessIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installment_FinancialDocument_FinancialDocumentId",
                schema: "Bill",
                table: "Installment",
                column: "FinancialDocumentId",
                principalSchema: "Bill",
                principalTable: "FinancialDocument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installment_Tenant_TenantId",
                schema: "Bill",
                table: "Installment",
                column: "TenantId",
                principalSchema: "Bill",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInstallment_Installment_Id",
                schema: "Bill",
                table: "MerchantInstallment",
                column: "Id",
                principalSchema: "Bill",
                principalTable: "Installment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billing_Billing_CreditorId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropForeignKey(
                name: "FK_Billing_Billing_DebtorId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropForeignKey(
                name: "FK_Installment_BusinessIdentity_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropForeignKey(
                name: "FK_Installment_BusinessIdentity_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropForeignKey(
                name: "FK_Installment_FinancialDocument_FinancialDocumentId",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropForeignKey(
                name: "FK_Installment_Tenant_TenantId",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropForeignKey(
                name: "FK_MerchantInstallment_Installment_Id",
                schema: "Bill",
                table: "MerchantInstallment");

            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Bill");

            migrationBuilder.DropIndex(
                name: "IX_Billing_DebtorId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Installment",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropColumn(
                name: "AdditionsAmount",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "AdditionsDescription",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "DeductionsAmount",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "DeductionsDescription",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "MainContractId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "Transferred",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "CashAmount",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropColumn(
                name: "CreditAmount",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.DropColumn(
                name: "PrepaymentAmount",
                schema: "Bill",
                table: "Installment");

            migrationBuilder.RenameTable(
                name: "Installment",
                schema: "Bill",
                newName: "Installments",
                newSchema: "Bill");

            migrationBuilder.RenameColumn(
                name: "RefundedTransactionsAmount",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "PreviousPeriodRefundedTransactions");

            migrationBuilder.RenameColumn(
                name: "PurchaseTransactionsCommission",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "Deductions");

            migrationBuilder.RenameColumn(
                name: "PurchaseTransactionsCalculatedCommission",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "CurrentPeriodPurchaseTransactions");

            migrationBuilder.RenameColumn(
                name: "PurchaseTransactionsAmount",
                schema: "Bill",
                table: "MerchantBilling",
                newName: "CurrentPeriodFinalCommission");

            migrationBuilder.RenameColumn(
                name: "DebtorId",
                schema: "Bill",
                table: "Billing",
                newName: "ParentId");

            migrationBuilder.RenameColumn(
                name: "CreditorId",
                schema: "Bill",
                table: "Billing",
                newName: "BillingId");

            migrationBuilder.RenameIndex(
                name: "IX_Billing_CreditorId",
                schema: "Bill",
                table: "Billing",
                newName: "IX_Billing_BillingId");

            migrationBuilder.RenameIndex(
                name: "IX_Installment_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installments",
                newName: "IX_Installments_ToBusinessIdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Installment_TenantId",
                schema: "Bill",
                table: "Installments",
                newName: "IX_Installments_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Installment_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installments",
                newName: "IX_Installments_FromBusinessIdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Installment_FinancialDocumentId",
                schema: "Bill",
                table: "Installments",
                newName: "IX_Installments_FinancialDocumentId");

            migrationBuilder.AddColumn<decimal>(
                name: "Additions",
                schema: "Bill",
                table: "MerchantBilling",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AdditionsDescription",
                schema: "Bill",
                table: "MerchantBilling",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPeriodCalculatedCommission",
                schema: "Bill",
                table: "MerchantBilling",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeductionsDescription",
                schema: "Bill",
                table: "MerchantBilling",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MerchantId",
                schema: "Bill",
                table: "MerchantBilling",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Commission",
                schema: "Bill",
                table: "FinancialDocument",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(32,10)");

            migrationBuilder.AlterColumn<string>(
                name: "ContractIds",
                schema: "Bill",
                table: "Billing",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<long>(
                name: "BillingId",
                schema: "Bill",
                table: "Installments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantMerchantContractId1",
                schema: "Bill",
                table: "Installments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Installments",
                schema: "Bill",
                table: "Installments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantBilling_MerchantId",
                schema: "Bill",
                table: "MerchantBilling",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_ParentId",
                schema: "Bill",
                table: "Billing",
                column: "ParentId",
                unique: true,
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Installments_BillingId",
                schema: "Bill",
                table: "Installments",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_Installments_TenantMerchantContractId1",
                schema: "Bill",
                table: "Installments",
                column: "TenantMerchantContractId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_Billing_BillingId",
                schema: "Bill",
                table: "Billing",
                column: "BillingId",
                principalSchema: "Bill",
                principalTable: "Billing",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_Billing_ParentId",
                schema: "Bill",
                table: "Billing",
                column: "ParentId",
                principalSchema: "Bill",
                principalTable: "Billing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Billing_BillingId",
                schema: "Bill",
                table: "Installments",
                column: "BillingId",
                principalSchema: "Bill",
                principalTable: "Billing",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_BusinessIdentity_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installments",
                column: "FromBusinessIdentityId",
                principalSchema: "Bill",
                principalTable: "BusinessIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_BusinessIdentity_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installments",
                column: "ToBusinessIdentityId",
                principalSchema: "Bill",
                principalTable: "BusinessIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_FinancialDocument_FinancialDocumentId",
                schema: "Bill",
                table: "Installments",
                column: "FinancialDocumentId",
                principalSchema: "Bill",
                principalTable: "FinancialDocument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_TenantMerchantContract_TenantMerchantContractId1",
                schema: "Bill",
                table: "Installments",
                column: "TenantMerchantContractId1",
                principalSchema: "Bill",
                principalTable: "TenantMerchantContract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Tenant_TenantId",
                schema: "Bill",
                table: "Installments",
                column: "TenantId",
                principalSchema: "Bill",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantBilling_Merchant_MerchantId",
                schema: "Bill",
                table: "MerchantBilling",
                column: "MerchantId",
                principalSchema: "Bill",
                principalTable: "Merchant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantInstallment_Installments_Id",
                schema: "Bill",
                table: "MerchantInstallment",
                column: "Id",
                principalSchema: "Bill",
                principalTable: "Installments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
