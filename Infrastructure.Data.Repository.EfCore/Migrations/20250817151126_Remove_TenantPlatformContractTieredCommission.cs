using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Remove_TenantPlatformContractTieredCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "TenantPlatformContractTieredCommission",
            //    schema: "Fc");

            migrationBuilder.RenameColumn(
                name: "CommissionReferenceType",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "CommissionReferenceTypes");

            migrationBuilder.AddColumn<string>(
                name: "TieredCommissions",
                schema: "Fc",
                table: "TenantPlatformContract",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TieredCommissions",
                schema: "Fc",
                table: "TenantPlatformContract");

            migrationBuilder.RenameColumn(
                name: "CommissionReferenceTypes",
                schema: "Fc",
                table: "TenantPlatformContract",
                newName: "CommissionReferenceType");

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractTieredCommission",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    MinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractTieredCommission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractTieredCommission_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractTieredCommission_TenantPlatformContractId",
                schema: "Fc",
                table: "TenantPlatformContractTieredCommission",
                column: "TenantPlatformContractId");
        }
    }
}
