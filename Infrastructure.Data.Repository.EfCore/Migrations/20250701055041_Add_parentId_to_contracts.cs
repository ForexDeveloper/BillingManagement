using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_parentId_to_contracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ToAmount",
                schema: "Fc",
                table: "TenantPlatformContractTieredCommission",
                type: "decimal(32,10)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(32,10)");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                schema: "Fc",
                table: "TenantMerchantContract",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "Fc",
                table: "TenantMerchantContract");

            migrationBuilder.AlterColumn<decimal>(
                name: "ToAmount",
                schema: "Fc",
                table: "TenantPlatformContractTieredCommission",
                type: "decimal(32,10)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(32,10)",
                oldNullable: true);
        }
    }
}
