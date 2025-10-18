using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class add_calculatedTieredLevel_to_billing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TieredCalculatedLevels",
                schema: "Bill",
                table: "Billing",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TieredCalculatedLevels",
                schema: "Bill",
                table: "Billing");
        }
    }
}
