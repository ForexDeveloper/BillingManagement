using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_GracePeriod_Billing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GracePeriod",
                schema: "Fc",
                table: "Installment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                schema: "Fc",
                table: "FinancialDocument",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundDescription",
                schema: "Fc",
                table: "FinancialDocument",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "RefundReason",
                schema: "Fc",
                table: "FinancialDocument",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GracePeriod",
                schema: "Fc",
                table: "Billing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_ParentId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialDocument_FinancialDocument_ParentId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "ParentId",
                principalSchema: "Fc",
                principalTable: "FinancialDocument",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialDocument_FinancialDocument_ParentId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropIndex(
                name: "IX_FinancialDocument_ParentId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "GracePeriod",
                schema: "Fc",
                table: "Installment");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "RefundDescription",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                schema: "Fc",
                table: "FinancialDocument");

            migrationBuilder.DropColumn(
                name: "GracePeriod",
                schema: "Fc",
                table: "Billing");
        }
    }
}
