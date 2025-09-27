using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Cash_Credit_Prepayment_Amount_FinancialDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashAmount",
                schema: "Bill",
                table: "FinancialDocument",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditAmount",
                schema: "Bill",
                table: "FinancialDocument",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentAmount",
                schema: "Bill",
                table: "FinancialDocument",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashAmount",
                schema: "Bill",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "CreditAmount",
                schema: "Bill",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "PrepaymentAmount",
                schema: "Bill",
                table: "FinancialDocument");
        }
    }
}
