using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations.BillingDb
{
    /// <inheritdoc />
    public partial class Remove_TestColumn_BillingPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestColumn",
                schema: "Bill",
                table: "BillingPayment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TestColumn",
                schema: "Bill",
                table: "BillingPayment",
                type: "datetime2",
                nullable: true);
        }
    }
}
