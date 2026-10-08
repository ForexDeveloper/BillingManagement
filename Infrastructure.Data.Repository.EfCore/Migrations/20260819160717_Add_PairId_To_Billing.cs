using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations.BillingDb
{
    /// <inheritdoc />
    public partial class Add_PairId_To_Billing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PairId",
                schema: "Bill",
                table: "Billing",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billing_PairId",
                schema: "Bill",
                table: "Billing",
                column: "PairId",
                unique: true,
                filter: "[PairId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_Billing_PairId",
                schema: "Bill",
                table: "Billing",
                column: "PairId",
                principalSchema: "Bill",
                principalTable: "Billing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billing_Billing_PairId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropIndex(
                name: "IX_Billing_PairId",
                schema: "Bill",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "PairId",
                schema: "Bill",
                table: "Billing");
        }
    }
}
