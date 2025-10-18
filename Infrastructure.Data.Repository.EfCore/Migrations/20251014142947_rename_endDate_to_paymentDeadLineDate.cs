using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class rename_endDate_to_paymentDeadLineDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "Bill",
                table: "Billing",
                newName: "PaymentDeadlineDate");

            migrationBuilder.AddColumn<long>(
                name: "PaymentId",
                schema: "Bill",
                table: "BillingPayment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_PaymentId",
                schema: "Bill",
                table: "BillingPayment",
                column: "PaymentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillingPayment_PaymentId",
                schema: "Bill",
                table: "BillingPayment");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                schema: "Bill",
                table: "BillingPayment");

            migrationBuilder.RenameColumn(
                name: "PaymentDeadlineDate",
                schema: "Bill",
                table: "Billing",
                newName: "EndDate");
        }
    }
}
