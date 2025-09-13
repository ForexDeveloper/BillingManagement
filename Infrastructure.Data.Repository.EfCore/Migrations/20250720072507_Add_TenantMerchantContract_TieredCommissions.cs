using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_TenantMerchantContract_TieredCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantMerchantContractInterest",
                schema: "Fc");

            migrationBuilder.DropColumn(
                name: "Commission",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "CommissionFeeType",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "SignatureOwners",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.RenameColumn(
                name: "Subject",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "InterestReferenceTypes");

            migrationBuilder.RenameColumn(
                name: "SettlementInstallmentsPeriod",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "InstallmentsCount");

            migrationBuilder.RenameColumn(
                name: "SettlementInstallmentsCount",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "CommissionDeductionMethodType");

            migrationBuilder.RenameColumn(
                name: "SettlementDay",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "CommissionCalculationType");

            migrationBuilder.RenameColumn(
                name: "PeriodType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "SettlementType");

            migrationBuilder.RenameColumn(
                name: "PeriodOriginDate",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "DailyBillingOriginDate");

            migrationBuilder.RenameColumn(
                name: "MerchantSettlementType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "PaymentMethodType");

            migrationBuilder.RenameColumn(
                name: "CommissionPaymentType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "BillingPeriodType");

            migrationBuilder.RenameColumn(
                name: "CommissionMinAmount",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "TransactionMinCommissionAmount");

            migrationBuilder.RenameColumn(
                name: "CommissionMaxAmount",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "TransactionMaxCommissionAmount");

            migrationBuilder.RenameColumn(
                name: "CalculationDay",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "BillingPeriod");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "GuaranteeType",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingBreak",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionReferenceTypes",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "decimal(6,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestPercentage",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "decimal(6,3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommissionExchanged",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TieredCommissions",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingBreak",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "CommissionReferenceTypes",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "InterestPercentage",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "IsCommissionExchanged",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.DropColumn(
                name: "TieredCommissions",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.RenameColumn(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "CommissionMinAmount");

            migrationBuilder.RenameColumn(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "CommissionMaxAmount");

            migrationBuilder.RenameColumn(
                name: "SettlementType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "PeriodType");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "MerchantSettlementType");

            migrationBuilder.RenameColumn(
                name: "InterestReferenceTypes",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "InstallmentsCount",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "SettlementInstallmentsPeriod");

            migrationBuilder.RenameColumn(
                name: "DailyBillingOriginDate",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "PeriodOriginDate");

            migrationBuilder.RenameColumn(
                name: "CommissionDeductionMethodType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "SettlementInstallmentsCount");

            migrationBuilder.RenameColumn(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "SettlementDay");

            migrationBuilder.RenameColumn(
                name: "BillingPeriodType",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "CommissionPaymentType");

            migrationBuilder.RenameColumn(
                name: "BillingPeriod",
                schema: "Fc",
                table: "TenantMerchantContract",
                newName: "CalculationDay");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<byte>(
                name: "GuaranteeType",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "Commission",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte>(
                name: "CommissionFeeType",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "SignatureOwners",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TenantMerchantContractInterest",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantMerchantContractId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Interest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestCalculationType = table.Column<byte>(type: "tinyint", nullable: false),
                    InterestPortionType = table.Column<byte>(type: "tinyint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMerchantContractInterest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantMerchantContractInterest_TenantMerchantContract_TenantMerchantContractId",
                        column: x => x.TenantMerchantContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContractInterest_TenantMerchantContractId",
                schema: "Fc",
                table: "TenantMerchantContractInterest",
                column: "TenantMerchantContractId");
        }
    }
}
