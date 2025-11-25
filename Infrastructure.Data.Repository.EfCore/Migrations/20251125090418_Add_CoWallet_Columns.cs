using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations.BillingDb
{
    /// <inheritdoc />
    public partial class Add_CoWallet_Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAnonymous",
                schema: "Bill",
                table: "Tenant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCoWallet",
                schema: "Bill",
                table: "Tenant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CoWalletName",
                schema: "Bill",
                table: "Organization",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationIdentityType",
                schema: "Bill",
                table: "Organization",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationType",
                schema: "Bill",
                table: "Organization",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAnonymous",
                schema: "Bill",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "HasCoWallet",
                schema: "Bill",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "CoWalletName",
                schema: "Bill",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "OrganizationIdentityType",
                schema: "Bill",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "OrganizationType",
                schema: "Bill",
                table: "Organization");
        }
    }
}
