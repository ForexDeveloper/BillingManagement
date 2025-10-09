using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class add_billing_installment_payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Billing",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PreviousDebitAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PreviousCreditAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PreviousPenaltyAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    AdditionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    DeductionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    AdditionsDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeductionsDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GracePeriod = table.Column<int>(type: "int", nullable: false),
                    HasAttachment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ContractIds = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DebtorId = table.Column<long>(type: "bigint", nullable: true),
                    CreditorId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billing_Billing_CreditorId",
                        column: x => x.CreditorId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billing_Billing_DebtorId",
                        column: x => x.DebtorId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billing_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billing_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billing_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingPayment",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillingId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingPayment_Billing_BillingId",
                        column: x => x.BillingId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Installment",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FinancialDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    BillingId = table.Column<long>(type: "bigint", nullable: true),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CashAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PrepaymentAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installment_Billing_BillingId",
                        column: x => x.BillingId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Installment_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installment_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installment_FinancialDocument_FinancialDocumentId",
                        column: x => x.FinancialDocumentId,
                        principalSchema: "Bill",
                        principalTable: "FinancialDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installment_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantBilling",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseTransactionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    RefundedTransactionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PurchaseTransactionsCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    RefundedTransactionsCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PurchaseTransactionsCalculatedCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBilling", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantBilling_Billing_Id",
                        column: x => x.Id,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantInstallment",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TenantMerchantContractId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantInstallment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantInstallment_Installment_Id",
                        column: x => x.Id,
                        principalSchema: "Bill",
                        principalTable: "Installment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantInstallment_TenantMerchantContract_TenantMerchantContractId",
                        column: x => x.TenantMerchantContractId,
                        principalSchema: "Bill",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billing_CreditorId",
                schema: "Bill",
                table: "Billing",
                column: "CreditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_DebtorId",
                schema: "Bill",
                table: "Billing",
                column: "DebtorId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_FromBusinessIdentityId",
                schema: "Bill",
                table: "Billing",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_TenantId",
                schema: "Bill",
                table: "Billing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_ToBusinessIdentityId",
                schema: "Bill",
                table: "Billing",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_BillingId",
                schema: "Bill",
                table: "BillingPayment",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_BillingId",
                schema: "Bill",
                table: "Installment",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_FinancialDocumentId",
                schema: "Bill",
                table: "Installment",
                column: "FinancialDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_TenantId",
                schema: "Bill",
                table: "Installment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInstallment_TenantMerchantContractId",
                schema: "Bill",
                table: "MerchantInstallment",
                column: "TenantMerchantContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingPayment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "MerchantBilling",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "MerchantInstallment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Installment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Billing",
                schema: "Bill");
        }
    }
}
