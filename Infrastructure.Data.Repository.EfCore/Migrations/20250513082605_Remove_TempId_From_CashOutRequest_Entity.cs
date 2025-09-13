using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Remove_TempId_From_CashOutRequest_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CashOutRequest",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "TempId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashOutRequest",
                schema: "Fc",
                table: "CashOutRequest",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CashOutRequest",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.AddColumn<int>(
                name: "TempId",
                schema: "Fc",
                table: "CashOutRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashOutRequest",
                schema: "Fc",
                table: "CashOutRequest",
                column: "TempId");
        }
    }
}
