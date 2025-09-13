using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Change_MerchantBranch_To_Inherit_From_BusinessIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                schema: "Fc",
                table: "MerchantBranch");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                schema: "Fc",
                table: "MerchantBranch");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                schema: "Fc",
                table: "MerchantBranch");

            migrationBuilder.DropColumn(
                name: "EditDateTime",
                schema: "Fc",
                table: "MerchantBranch");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Fc",
                table: "MerchantBranch");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantBranch_BusinessIdentity_Id",
                schema: "Fc",
                table: "MerchantBranch",
                column: "Id",
                principalSchema: "Fc",
                principalTable: "BusinessIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantBranch_BusinessIdentity_Id",
                schema: "Fc",
                table: "MerchantBranch");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                schema: "Fc",
                table: "MerchantBranch",
                type: "VARCHAR(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTime",
                schema: "Fc",
                table: "MerchantBranch",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                schema: "Fc",
                table: "MerchantBranch",
                type: "VARCHAR(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditDateTime",
                schema: "Fc",
                table: "MerchantBranch",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Fc",
                table: "MerchantBranch",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
