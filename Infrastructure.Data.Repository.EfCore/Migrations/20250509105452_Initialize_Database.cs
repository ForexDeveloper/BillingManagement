using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Fc");

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    FileReference = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    AttachmentCategory = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccount",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BusinessIdentity = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessIdentity",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessIdentity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Category_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Fc",
                        principalTable: "Category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurrencyType",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Outbox",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishTryCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provider",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderType = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provider", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Merchant",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    SaleType = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Merchant_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreditProjectName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InternalProjectManagerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenant_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantBranch",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TerminalId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantBranch_Merchant_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "Fc",
                        principalTable: "Merchant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantCategory",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Fc",
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MerchantCategory_Merchant_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "Fc",
                        principalTable: "Merchant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    BusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    NonWithDrawableBalance = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_BusinessIdentity_BusinessIdentityId",
                        column: x => x.BusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Account_CurrencyType_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "Fc",
                        principalTable: "CurrencyType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Account_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Customer_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Facilitator",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsTenant = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilitator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facilitator_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Facilitator_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialDocument",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialDocument_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialDocument_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialDocument_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Financier",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsTenant = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Financier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Financier_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Financier_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Guarantor",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsTenant = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guarantor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guarantor_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guarantor_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organization_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organization_Organization_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Fc",
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Organization_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectManager",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectManager", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectManager_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantIpgSetting",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IpgType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantIpgSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantIpgSetting_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantMerchantContract",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    EnamadLink = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InternetBusinessLicenseLink = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    GuaranteeType = table.Column<byte>(type: "tinyint", nullable: false),
                    GuaranteeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodOriginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalculationDay = table.Column<int>(type: "int", nullable: false),
                    MerchantSettlementType = table.Column<byte>(type: "tinyint", nullable: false),
                    SettlementDay = table.Column<int>(type: "int", nullable: false),
                    SettlementInstallmentsCount = table.Column<int>(type: "int", nullable: true),
                    SettlementInstallmentsPeriod = table.Column<int>(type: "int", nullable: true),
                    CommissionFeeType = table.Column<byte>(type: "tinyint", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CommissionPaymentType = table.Column<byte>(type: "tinyint", nullable: false),
                    CommissionMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    CommissionMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SignatureOwners = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMerchantContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantMerchantContract_Merchant_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "Fc",
                        principalTable: "Merchant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantMerchantContract_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Billing",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PerviousDebitAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PerviousCreditAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PerviousPenaltyAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billing_Account_FromAccountId",
                        column: x => x.FromAccountId,
                        principalSchema: "Fc",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billing_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Billing_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    ToAccountId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    DueDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Account_FromAccountId",
                        column: x => x.FromAccountId,
                        principalSchema: "Fc",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_Account_ToAccountId",
                        column: x => x.ToAccountId,
                        principalSchema: "Fc",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_Transaction_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Fc",
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerOrganization",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrganization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerOrganization_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Fc",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerOrganization_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Fc",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletConfiguration",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    WalletTypeId = table.Column<byte>(type: "tinyint", nullable: false),
                    MaxWallet = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    MaxTotalCredit = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ProjectManagerId = table.Column<int>(type: "int", nullable: true),
                    CurrencyTypeId = table.Column<int>(type: "int", nullable: true),
                    MaxInstallments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrepaymentMinPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PrepaymentMaxPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PrepaymentMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PrepaymentMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    InterestPeriodMaxPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    InterestPeriodMinPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    InterestPeriodMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    InterestPeriodMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PenaltyPeriodMaxPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PenaltyPeriodMinPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PenaltyPeriodMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PenaltyPeriodMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    WaiverPeriodMaxPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    WaiverPeriodMinPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    WaiverPeriodMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    WaiverPeriodMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletConfiguration_CurrencyType_CurrencyTypeId",
                        column: x => x.CurrencyTypeId,
                        principalSchema: "Fc",
                        principalTable: "CurrencyType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletConfiguration_ProjectManager_ProjectManagerId",
                        column: x => x.ProjectManagerId,
                        principalSchema: "Fc",
                        principalTable: "ProjectManager",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletConfiguration_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContract",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PlatformFeeMethodType = table.Column<byte>(type: "tinyint", nullable: false),
                    CommissionCalculationType = table.Column<byte>(type: "tinyint", nullable: true),
                    TransactionFeeType = table.Column<byte>(type: "tinyint", nullable: true),
                    Commission = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    SubscriptionFeeType = table.Column<byte>(type: "tinyint", nullable: true),
                    FullUpfrontPaymentAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PerTransactionPaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    SubscriptionsPaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    PerTransactionChequeDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionsChequeDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InstallmentsCount = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    TenantIpgSettingId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContract_TenantIpgSetting_TenantIpgSettingId",
                        column: x => x.TenantIpgSettingId,
                        principalSchema: "Fc",
                        principalTable: "TenantIpgSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContract_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContract",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    TenantIpgSettingId = table.Column<int>(type: "int", nullable: true),
                    GrantingProcessId = table.Column<int>(type: "int", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContract_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Fc",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletContract_TenantIpgSetting_TenantIpgSettingId",
                        column: x => x.TenantIpgSettingId,
                        principalSchema: "Fc",
                        principalTable: "TenantIpgSetting",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletContract_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantMerchantContractInterest",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantMerchantContractId = table.Column<int>(type: "int", nullable: false),
                    InterestCalculationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Interest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPortionType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMerchantContractInterest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantMerchantContractInterest_TenantMerchantContract_TenantMerchantContractId",
                        column: x => x.TenantMerchantContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingPayment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillingId = table.Column<long>(type: "bigint", nullable: false),
                    PayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentDetailId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingPayment_Billing_BillingId",
                        column: x => x.BillingId,
                        principalSchema: "Fc",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillingPayment_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Fc",
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Installment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    ToAccountId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    HasBilling = table.Column<bool>(type: "bit", nullable: false),
                    LastPenaltyCalculationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installment_Account_FromAccountId",
                        column: x => x.FromAccountId,
                        principalSchema: "Fc",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installment_Account_ToAccountId",
                        column: x => x.ToAccountId,
                        principalSchema: "Fc",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installment_Installment_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Fc",
                        principalTable: "Installment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Installment_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Installment_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Fc",
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Closedloop",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletConfigurationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Closedloop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Closedloop_WalletConfiguration_WalletConfigurationId",
                        column: x => x.WalletConfigurationId,
                        principalSchema: "Fc",
                        principalTable: "WalletConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plan",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletConfigurationId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MaxDailyWithdrawal = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    MaxWallet = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    MaxTotalCredit = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    BillingPeriodType = table.Column<byte>(type: "tinyint", nullable: true),
                    BillingPeriod = table.Column<int>(type: "int", nullable: true),
                    BillingPeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GracePeriod = table.Column<int>(type: "int", nullable: true),
                    PaymentType = table.Column<byte>(type: "tinyint", nullable: true),
                    InstallmentBreakType = table.Column<byte>(type: "tinyint", nullable: true),
                    InstallmentBreak = table.Column<int>(type: "int", nullable: true),
                    InstallmentPaymentMethod = table.Column<byte>(type: "tinyint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxDailyDeposit = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    MaxDailyTransactionCount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BackgroundColor1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BackgroundColor2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TextColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plan_WalletConfiguration_WalletConfigurationId",
                        column: x => x.WalletConfigurationId,
                        principalSchema: "Fc",
                        principalTable: "WalletConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractInstallment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    DueDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractInstallment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractInstallment_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractProvider",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractProvider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractProvider_Provider_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "Fc",
                        principalTable: "Provider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractProvider_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContractBusinessIdentity",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    BusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    HasWallet = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractBusinessIdentity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractBusinessIdentity_BusinessIdentity_BusinessIdentityId",
                        column: x => x.BusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletContractBusinessIdentity_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContractFacilitator",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: false),
                    PortionType = table.Column<byte>(type: "tinyint", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractFacilitator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractFacilitator_Facilitator_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalSchema: "Fc",
                        principalTable: "Facilitator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletContractFacilitator_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContractFinancier",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    FinancierId = table.Column<int>(type: "int", nullable: false),
                    PortionType = table.Column<byte>(type: "tinyint", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractFinancier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractFinancier_Financier_FinancierId",
                        column: x => x.FinancierId,
                        principalSchema: "Fc",
                        principalTable: "Financier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletContractFinancier_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContractGuarantor",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    GuarantorId = table.Column<int>(type: "int", nullable: false),
                    PortionType = table.Column<byte>(type: "tinyint", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractGuarantor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractGuarantor_Guarantor_GuarantorId",
                        column: x => x.GuarantorId,
                        principalSchema: "Fc",
                        principalTable: "Guarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletContractGuarantor_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContractRejectionReason",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractRejectionReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractRejectionReason_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingInstallment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillingId = table.Column<long>(type: "bigint", nullable: false),
                    InstallmentId = table.Column<long>(type: "bigint", nullable: false),
                    IsPrevious = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingInstallment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingInstallment_Billing_BillingId",
                        column: x => x.BillingId,
                        principalSchema: "Fc",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillingInstallment_Installment_InstallmentId",
                        column: x => x.InstallmentId,
                        principalSchema: "Fc",
                        principalTable: "Installment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClosedloopCategory",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClosedloopId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedloopCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosedloopCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Fc",
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClosedloopCategory_Closedloop_ClosedloopId",
                        column: x => x.ClosedloopId,
                        principalSchema: "Fc",
                        principalTable: "Closedloop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClosedloopMerchant",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClosedloopId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedloopMerchant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosedloopMerchant_Closedloop_ClosedloopId",
                        column: x => x.ClosedloopId,
                        principalSchema: "Fc",
                        principalTable: "Closedloop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClosedloopMerchant_Merchant_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "Fc",
                        principalTable: "Merchant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanClosedloop",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClosedLoopId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanClosedloop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanClosedloop_Closedloop_ClosedLoopId",
                        column: x => x.ClosedLoopId,
                        principalSchema: "Fc",
                        principalTable: "Closedloop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanClosedloop_Plan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "Fc",
                        principalTable: "Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanDetail",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    OperationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OperationalFeeType = table.Column<byte>(type: "tinyint", nullable: true),
                    PenaltyPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PenaltyMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PenaltyMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    InterestPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    InterestMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    InterestMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    WaiverPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    WaiverMaxAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    WaiverMinAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PrepaymentPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PrepaymentMinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PrepaymentMaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanDetail_Plan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "Fc",
                        principalTable: "Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallet",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    BusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallet_Account_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "Fc",
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wallet_BusinessIdentity_BusinessIdentityId",
                        column: x => x.BusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wallet_Plan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "Fc",
                        principalTable: "Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wallet_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Fc",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wallet_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletContractPlan",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractPlan_Plan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "Fc",
                        principalTable: "Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletContractPlan_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanDetailInstallment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanDetailId = table.Column<int>(type: "int", nullable: false),
                    NumberOfInstallment = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDetailInstallment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanDetailInstallment_PlanDetail_PlanDetailId",
                        column: x => x.PlanDetailId,
                        principalSchema: "Fc",
                        principalTable: "PlanDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashWallet",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashWallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashWallet_Wallet_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "Wallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialDocumentPayment",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    WalletId = table.Column<int>(type: "int", nullable: true),
                    WalletContractId = table.Column<int>(type: "int", nullable: true),
                    MerchantContractId = table.Column<int>(type: "int", nullable: true),
                    FinancialDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentDetailId = table.Column<long>(type: "bigint", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialDocumentPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Fc",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_FinancialDocument_FinancialDocumentId",
                        column: x => x.FinancialDocumentId,
                        principalSchema: "Fc",
                        principalTable: "FinancialDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_TenantMerchantContract_MerchantContractId",
                        column: x => x.MerchantContractId,
                        principalSchema: "Fc",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Fc",
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Fc",
                        principalTable: "WalletContract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocumentPayment_Wallet_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "Fc",
                        principalTable: "Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LoanWallet",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OperationalFee = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    OperationalFeeType = table.Column<byte>(type: "tinyint", nullable: false),
                    InitialAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    NumberOfInstallment = table.Column<int>(type: "int", nullable: false),
                    UserCreditGrantingProcessId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanWallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanWallet_Wallet_Id",
                        column: x => x.Id,
                        principalSchema: "Fc",
                        principalTable: "Wallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashOutRequests",
                schema: "Fc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashWalletId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RejectReason = table.Column<byte>(type: "tinyint", nullable: false),
                    RejectDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankTransactionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashOutRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashOutRequests_BankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "Fc",
                        principalTable: "BankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashOutRequests_CashWallet_CashWalletId",
                        column: x => x.CashWalletId,
                        principalSchema: "Fc",
                        principalTable: "CashWallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashOutRequests_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Fc",
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_BusinessIdentityId",
                schema: "Fc",
                table: "Account",
                column: "BusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_CurrencyId",
                schema: "Fc",
                table: "Account",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_TenantId",
                schema: "Fc",
                table: "Account",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_FromAccountId",
                schema: "Fc",
                table: "Billing",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_TenantId",
                schema: "Fc",
                table: "Billing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_ToBusinessIdentityId",
                schema: "Fc",
                table: "Billing",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingInstallment_BillingId",
                schema: "Fc",
                table: "BillingInstallment",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingInstallment_InstallmentId",
                schema: "Fc",
                table: "BillingInstallment",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_BillingId",
                schema: "Fc",
                table: "BillingPayment",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_TransactionId",
                schema: "Fc",
                table: "BillingPayment",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashOutRequests_BankAccountId",
                schema: "Fc",
                table: "CashOutRequests",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashOutRequests_CashWalletId",
                schema: "Fc",
                table: "CashOutRequests",
                column: "CashWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_CashOutRequests_TransactionId",
                schema: "Fc",
                table: "CashOutRequests",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentId",
                schema: "Fc",
                table: "Category",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Closedloop_WalletConfigurationId",
                schema: "Fc",
                table: "Closedloop",
                column: "WalletConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedloopCategory_CategoryId",
                schema: "Fc",
                table: "ClosedloopCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedloopCategory_ClosedloopId",
                schema: "Fc",
                table: "ClosedloopCategory",
                column: "ClosedloopId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedloopMerchant_ClosedloopId",
                schema: "Fc",
                table: "ClosedloopMerchant",
                column: "ClosedloopId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedloopMerchant_MerchantId",
                schema: "Fc",
                table: "ClosedloopMerchant",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_TenantId",
                schema: "Fc",
                table: "Customer",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrganization_CustomerId",
                schema: "Fc",
                table: "CustomerOrganization",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrganization_OrganizationId",
                schema: "Fc",
                table: "CustomerOrganization",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Facilitator_TenantId",
                schema: "Fc",
                table: "Facilitator",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_FromBusinessIdentityId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_TenantId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_ToBusinessIdentityId",
                schema: "Fc",
                table: "FinancialDocument",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_FinancialDocumentId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "FinancialDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_FromBusinessIdentityId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_MerchantContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "MerchantContractId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_ToBusinessIdentityId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_TransactionId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_WalletContractId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocumentPayment_WalletId",
                schema: "Fc",
                table: "FinancialDocumentPayment",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Financier_TenantId",
                schema: "Fc",
                table: "Financier",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Guarantor_TenantId",
                schema: "Fc",
                table: "Guarantor",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_FromAccountId",
                schema: "Fc",
                table: "Installment",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_ParentId",
                schema: "Fc",
                table: "Installment",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_TenantId",
                schema: "Fc",
                table: "Installment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_ToAccountId",
                schema: "Fc",
                table: "Installment",
                column: "ToAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_TransactionId",
                schema: "Fc",
                table: "Installment",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantBranch_MerchantId",
                schema: "Fc",
                table: "MerchantBranch",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCategory_CategoryId",
                schema: "Fc",
                table: "MerchantCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCategory_MerchantId",
                schema: "Fc",
                table: "MerchantCategory",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ParentId",
                schema: "Fc",
                table: "Organization",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_TenantId",
                schema: "Fc",
                table: "Organization",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_WalletConfigurationId",
                schema: "Fc",
                table: "Plan",
                column: "WalletConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanClosedloop_ClosedLoopId",
                schema: "Fc",
                table: "PlanClosedloop",
                column: "ClosedLoopId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanClosedloop_PlanId",
                schema: "Fc",
                table: "PlanClosedloop",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDetail_PlanId",
                schema: "Fc",
                table: "PlanDetail",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDetailInstallment_PlanDetailId",
                schema: "Fc",
                table: "PlanDetailInstallment",
                column: "PlanDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectManager_TenantId",
                schema: "Fc",
                table: "ProjectManager",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantIpgSetting_TenantId",
                schema: "Fc",
                table: "TenantIpgSetting",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContract_MerchantId",
                schema: "Fc",
                table: "TenantMerchantContract",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContract_TenantId",
                schema: "Fc",
                table: "TenantMerchantContract",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContractInterest_TenantMerchantContractId",
                schema: "Fc",
                table: "TenantMerchantContractInterest",
                column: "TenantMerchantContractId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContract_TenantId",
                schema: "Fc",
                table: "TenantPlatformContract",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContract_TenantIpgSettingId",
                schema: "Fc",
                table: "TenantPlatformContract",
                column: "TenantIpgSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractInstallment_TenantPlatformContractId",
                schema: "Fc",
                table: "TenantPlatformContractInstallment",
                column: "TenantPlatformContractId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractProvider_ProviderId",
                schema: "Fc",
                table: "TenantPlatformContractProvider",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractProvider_TenantPlatformContractId",
                schema: "Fc",
                table: "TenantPlatformContractProvider",
                column: "TenantPlatformContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FromAccountId",
                schema: "Fc",
                table: "Transaction",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ParentId",
                schema: "Fc",
                table: "Transaction",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TenantId",
                schema: "Fc",
                table: "Transaction",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ToAccountId",
                schema: "Fc",
                table: "Transaction",
                column: "ToAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_AccountId",
                schema: "Fc",
                table: "Wallet",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_BusinessIdentityId",
                schema: "Fc",
                table: "Wallet",
                column: "BusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_PlanId",
                schema: "Fc",
                table: "Wallet",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_TenantId",
                schema: "Fc",
                table: "Wallet",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_WalletContractId",
                schema: "Fc",
                table: "Wallet",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletConfiguration_CurrencyTypeId",
                schema: "Fc",
                table: "WalletConfiguration",
                column: "CurrencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletConfiguration_ProjectManagerId",
                schema: "Fc",
                table: "WalletConfiguration",
                column: "ProjectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletConfiguration_TenantId",
                schema: "Fc",
                table: "WalletConfiguration",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_OrganizationId",
                schema: "Fc",
                table: "WalletContract",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_TenantId",
                schema: "Fc",
                table: "WalletContract",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_TenantIpgSettingId",
                schema: "Fc",
                table: "WalletContract",
                column: "TenantIpgSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractBusinessIdentity_BusinessIdentityId",
                schema: "Fc",
                table: "WalletContractBusinessIdentity",
                column: "BusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractBusinessIdentity_WalletContractId",
                schema: "Fc",
                table: "WalletContractBusinessIdentity",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFacilitator_FacilitatorId",
                schema: "Fc",
                table: "WalletContractFacilitator",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFacilitator_WalletContractId",
                schema: "Fc",
                table: "WalletContractFacilitator",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFinancier_FinancierId",
                schema: "Fc",
                table: "WalletContractFinancier",
                column: "FinancierId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFinancier_WalletContractId",
                schema: "Fc",
                table: "WalletContractFinancier",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractGuarantor_GuarantorId",
                schema: "Fc",
                table: "WalletContractGuarantor",
                column: "GuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractGuarantor_WalletContractId",
                schema: "Fc",
                table: "WalletContractGuarantor",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractPlan_PlanId",
                schema: "Fc",
                table: "WalletContractPlan",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractPlan_WalletContractId",
                schema: "Fc",
                table: "WalletContractPlan",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractRejectionReason_WalletContractId",
                schema: "Fc",
                table: "WalletContractRejectionReason",
                column: "WalletContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "BillingInstallment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "BillingPayment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "CashOutRequests",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "ClosedloopCategory",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "ClosedloopMerchant",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "CustomerOrganization",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "FinancialDocumentPayment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "LoanWallet",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "MerchantBranch",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "MerchantCategory",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Outbox",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "PlanClosedloop",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "PlanDetailInstallment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantMerchantContractInterest",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantPlatformContractInstallment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantPlatformContractProvider",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContractBusinessIdentity",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContractFacilitator",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContractFinancier",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContractGuarantor",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContractPlan",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContractRejectionReason",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Installment",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Billing",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "BankAccount",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "CashWallet",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "FinancialDocument",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Category",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Closedloop",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "PlanDetail",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantMerchantContract",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Provider",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantPlatformContract",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Facilitator",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Financier",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Guarantor",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Transaction",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Wallet",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Merchant",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Account",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Plan",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletContract",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "WalletConfiguration",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Organization",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "TenantIpgSetting",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "CurrencyType",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "ProjectManager",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "Fc");

            migrationBuilder.DropTable(
                name: "BusinessIdentity",
                schema: "Fc");
        }
    }
}
