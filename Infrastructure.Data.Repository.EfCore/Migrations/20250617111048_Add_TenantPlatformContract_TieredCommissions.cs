using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_TenantPlatformContract_TieredCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantPlatformContractInstallment",
                schema: "Fc");

            migrationBuilder.DropColumn(
                name: "PerTransactionChequeDueDate",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "PerTransactionPaymentMethodType",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "SubscriptionFeeType",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "SubscriptionsChequeDueDate",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "SubscriptionsPaymentMethodType",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "TransactionFeeType",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.RenameColumn(
                name: "PlatformFeeMethodType",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "FeeCalculationType");

            migrationBuilder.RenameColumn(
                name: "InstallmentsCount",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "GracePeriod");

            migrationBuilder.RenameColumn(
                name: "FullUpfrontPaymentAmount",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "TransactionMinCommissionAmount");

            migrationBuilder.RenameColumn(
                name: "Commission",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "TransactionMaxCommissionAmount");

            migrationBuilder.AlterColumn<int>(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingPeriod",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "BillingPeriodType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "CommissionReferenceType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedAmount",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "decimal(6,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyPercent",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "decimal(32,10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractFacilitator",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: false),
                    FixedAmountCommissionPercentage = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    TransactionsCommissionPercentage = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractFacilitator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractFacilitator_Facilitator_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalSchema: "Fc",
                        principalTable: "Facilitator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractFacilitator_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractTieredCommission",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractTieredCommission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractTieredCommission_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractFacilitator_FacilitatorId",
                schema: "Fc",
                table: "TenantPlatformContractFacilitator",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractFacilitator_TenantPlatformContractId",
                schema: "Fc",
                table: "TenantPlatformContractFacilitator",
                column: "TenantPlatformContractId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractTieredCommission_TenantPlatformContractId",
                schema: "Fc",
                table: "TenantPlatformContractTieredCommission",
                column: "TenantPlatformContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantPlatformContractFacilitator",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantPlatformContractTieredCommission",
                schema: "Fc");

            migrationBuilder.DropColumn(
                name: "BillingPeriod",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "BillingPeriodType",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "CommissionReferenceType",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "FixedAmount",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "FixedAmountCommission",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "FixedPercentageCommission",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "PenaltyPercent",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "PeriodMaxCommissionAmount",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "PeriodMinCommissionAmount",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.RenameColumn(
                name: "TransactionMinCommissionAmount",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "FullUpfrontPaymentAmount");

            migrationBuilder.RenameColumn(
                name: "TransactionMaxCommissionAmount",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "Commission");

            migrationBuilder.RenameColumn(
                name: "GracePeriod",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "InstallmentsCount");

            migrationBuilder.RenameColumn(
                name: "FeeCalculationType",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "PlatformFeeMethodType");

            migrationBuilder.AlterColumn<byte>(
                name: "CommissionCalculationType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "PerTransactionChequeDueDate",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "PerTransactionPaymentMethodType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SubscriptionFeeType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionsChequeDueDate",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SubscriptionsPaymentMethodType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TransactionFeeType",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "tinyint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractInstallment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    DueDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractInstallment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractInstallment_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractInstallment_TenantPlatformContractId",
                schema: "Fc",
                table: "TenantPlatformContractInstallment",
                column: "TenantPlatformContractId");
        }
    }
}
