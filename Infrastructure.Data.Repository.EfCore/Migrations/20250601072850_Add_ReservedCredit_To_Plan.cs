using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_ReservedCredit_To_Plan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AssignedCredit",
                schema: "Fc",
                table: "Plan",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedCredit",
                schema: "Fc",
                table: "Plan",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedCredit",
                schema: "Fc",
                table: "Plan");

            migrationBuilder.DropColumn(
                name: "ReservedCredit",
                schema: "Fc",
                table: "Plan");
        }
    }
}
