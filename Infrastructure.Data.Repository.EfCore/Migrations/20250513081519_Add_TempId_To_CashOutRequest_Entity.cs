using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_TempId_To_CashOutRequest_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequests_BankAccount_BankAccountId",
                schema: "Fc",
                table: "CashOutRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequests_CashWallet_CashWalletId",
                schema: "Fc",
                table: "CashOutRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequests_Transaction_TransactionId",
                schema: "Fc",
                table: "CashOutRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CashOutRequests",
                schema: "Fc",
                table: "CashOutRequests");

            migrationBuilder.DropColumn(
                name: "RejectDescription",
                schema: "Fc",
                table: "CashOutRequests");

            migrationBuilder.RenameTable(
                name: "CashOutRequests",
                schema: "Fc",
                newName: "CashOutRequest",
                newSchema: "Fc");

            migrationBuilder.RenameColumn(
                name: "BusinessIdentity",
                schema: "Fc",
                table: "BankAccount",
                newName: "BusinessIdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_CashOutRequests_TransactionId",
                schema: "Fc",
                table: "CashOutRequest",
                newName: "IX_CashOutRequest_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_CashOutRequests_CashWalletId",
                schema: "Fc",
                table: "CashOutRequest",
                newName: "IX_CashOutRequest_CashWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_CashOutRequests_BankAccountId",
                schema: "Fc",
                table: "CashOutRequest",
                newName: "IX_CashOutRequest_BankAccountId");

            migrationBuilder.CreateSequence(
                name: "FollowUpCode",
                schema: "Fc",
                startValue: 1000000L);

            migrationBuilder.AlterColumn<string>(
                name: "BankTransactionCode",
                schema: "Fc",
                table: "CashOutRequest",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "Fc",
                table: "CashOutRequest",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "TempId",
                schema: "Fc",
                table: "CashOutRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Fc",
                table: "CashOutRequest",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FollowUpCode",
                schema: "Fc",
                table: "CashOutRequest",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR Fc.FollowUpCode");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
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

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_BusinessIdentityId",
                schema: "Fc",
                table: "BankAccount",
                column: "BusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_CashOutRequest_TenantId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccount_BusinessIdentity_BusinessIdentityId",
                schema: "Fc",
                table: "BankAccount",
                column: "BusinessIdentityId",
                principalSchema: "Fc",
                principalTable: "BusinessIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequest_BankAccount_BankAccountId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "BankAccountId",
                principalSchema: "Fc",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequest_CashWallet_CashWalletId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "CashWalletId",
                principalSchema: "Fc",
                principalTable: "CashWallet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequest_Tenant_TenantId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "TenantId",
                principalSchema: "Fc",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequest_Transaction_TransactionId",
                schema: "Fc",
                table: "CashOutRequest",
                column: "TransactionId",
                principalSchema: "Fc",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccount_BusinessIdentity_BusinessIdentityId",
                schema: "Fc",
                table: "BankAccount");

            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequest_BankAccount_BankAccountId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequest_CashWallet_CashWalletId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequest_Tenant_TenantId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_CashOutRequest_Transaction_TransactionId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropIndex(
                name: "IX_BankAccount_BusinessIdentityId",
                schema: "Fc",
                table: "BankAccount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CashOutRequest",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropIndex(
                name: "IX_CashOutRequest_TenantId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "TempId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "FollowUpCode",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Fc",
                table: "CashOutRequest");

            migrationBuilder.DropSequence(
                name: "FollowUpCode",
                schema: "Fc");

            migrationBuilder.RenameTable(
                name: "CashOutRequest",
                schema: "Fc",
                newName: "CashOutRequests",
                newSchema: "Fc");

            migrationBuilder.RenameColumn(
                name: "BusinessIdentityId",
                schema: "Fc",
                table: "BankAccount",
                newName: "BusinessIdentity");

            migrationBuilder.RenameIndex(
                name: "IX_CashOutRequest_TransactionId",
                schema: "Fc",
                table: "CashOutRequests",
                newName: "IX_CashOutRequests_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_CashOutRequest_CashWalletId",
                schema: "Fc",
                table: "CashOutRequests",
                newName: "IX_CashOutRequests_CashWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_CashOutRequest_BankAccountId",
                schema: "Fc",
                table: "CashOutRequests",
                newName: "IX_CashOutRequests_BankAccountId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Fc",
                table: "CashOutRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "BankTransactionCode",
                schema: "Fc",
                table: "CashOutRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectDescription",
                schema: "Fc",
                table: "CashOutRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashOutRequests",
                schema: "Fc",
                table: "CashOutRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequests_BankAccount_BankAccountId",
                schema: "Fc",
                table: "CashOutRequests",
                column: "BankAccountId",
                principalSchema: "Fc",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequests_CashWallet_CashWalletId",
                schema: "Fc",
                table: "CashOutRequests",
                column: "CashWalletId",
                principalSchema: "Fc",
                principalTable: "CashWallet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashOutRequests_Transaction_TransactionId",
                schema: "Fc",
                table: "CashOutRequests",
                column: "TransactionId",
                principalSchema: "Fc",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
