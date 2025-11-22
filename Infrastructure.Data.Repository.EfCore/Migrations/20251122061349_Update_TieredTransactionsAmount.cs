using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations.BillingDb
{
    /// <inheritdoc />
    public partial class Update_TieredTransactionsAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TieredTransactionsAmount",
                schema: "Bill",
                table: "Billing",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(32,10)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TieredTransactionsAmount",
                schema: "Bill",
                table: "Billing",
                type: "decimal(32,10)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(32,10)");
        }
    }
}
