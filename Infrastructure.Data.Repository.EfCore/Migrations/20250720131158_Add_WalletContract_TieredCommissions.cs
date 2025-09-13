using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_WalletContract_TieredCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(6,3)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TieredCommissions",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(6,3)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TieredCommissions",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(6,3)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TieredCommissions",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "decimal(32,10)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "TieredCommissions",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractGuarantor");

            migrationBuilder.DropColumn(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "TieredCommissions",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFinancier");

            migrationBuilder.DropColumn(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "TieredCommissions",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.DropColumn(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "WalletContractFacilitator");

            migrationBuilder.AlterColumn<byte>(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractGuarantor",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractFinancier",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PortionType",
                schema: "Fc",
                table: "WalletContractFacilitator",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
