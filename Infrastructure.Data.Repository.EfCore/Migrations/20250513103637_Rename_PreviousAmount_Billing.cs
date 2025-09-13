using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Rename_PreviousAmount_Billing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PerviousPenaltyAmount",
                schema: "Fc",
                table: "Billing",
                newName: "PreviousPenaltyAmount");

            migrationBuilder.RenameColumn(
                name: "PerviousDebitAmount",
                schema: "Fc",
                table: "Billing",
                newName: "PreviousDebitAmount");

            migrationBuilder.RenameColumn(
                name: "PerviousCreditAmount",
                schema: "Fc",
                table: "Billing",
                newName: "PreviousCreditAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousPenaltyAmount",
                schema: "Fc",
                table: "Billing",
                newName: "PerviousPenaltyAmount");

            migrationBuilder.RenameColumn(
                name: "PreviousDebitAmount",
                schema: "Fc",
                table: "Billing",
                newName: "PerviousDebitAmount");

            migrationBuilder.RenameColumn(
                name: "PreviousCreditAmount",
                schema: "Fc",
                table: "Billing",
                newName: "PerviousCreditAmount");
        }
    }
}
