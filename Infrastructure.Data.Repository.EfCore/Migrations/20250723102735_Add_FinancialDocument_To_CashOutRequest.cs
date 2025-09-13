using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_FinancialDocument_To_CashOutRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequest_Transaction_TransactionId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropIndex(
                name: "IX_CashOutRequest_TransactionId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.AlterColumn<long>(
                name: "PaymentDetailId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "PaymentId",
                schema: "Fc",
                table: "FinancialDocument",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "BankTransactionCode",
                schema: "Fc",
                table: "CashOutRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FinancialDocumentId",
                schema: "Fc",
                table: "CashOutRequest",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashOutRequest_FinancialDocumentId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "FinancialDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequest_FinancialDocument_FinancialDocumentId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "FinancialDocumentId",
                principalSchema: "Fc",
                principalTable: "FinancialDocument",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequest_FinancialDocument_FinancialDocumentId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropIndex(
                name: "IX_CashOutRequest_FinancialDocumentId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "FinancialDocumentId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.AlterColumn<long>(
                name: "PaymentDetailId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PaymentId",
                schema: "Fc",
                table: "FinancialDocument",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankTransactionCode",
                schema: "Fc",
                table: "CashOutRequest",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TransactionId",
                schema: "Fc",
                table: "CashOutRequest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_CashOutRequest_TransactionId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequest_Transaction_TransactionId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "TransactionId",
                principalSchema: "Fc",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
