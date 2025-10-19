using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Repository.EfCore.Migrations.BillingDb
{
    /// <inheritdoc />
    public partial class Modify_Billing_And_Installment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Bill");

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "Bill",
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
                    OriginalFileName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJob", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessIdentity",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessIdentity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Outbox",
                schema: "Bill",
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
                schema: "Bill",
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
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provider", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Merchant",
                schema: "Bill",
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
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "Bill",
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
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantBranch",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TerminalId = table.Column<long>(type: "bigint", nullable: false),
                    IsMerchant = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantBranch_BusinessIdentity_Id",
                        column: x => x.Id,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantBranch_Merchant_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "Bill",
                        principalTable: "Merchant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Billing",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    DebtorId = table.Column<long>(type: "bigint", nullable: true),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PreviousDebitAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PreviousCreditAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PreviousPenaltyAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    GracePeriod = table.Column<int>(type: "int", nullable: false),
                    ContractIds = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreditorId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    AdditionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    AdditionsDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeductionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    DeductionsDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MainContractId = table.Column<int>(type: "int", nullable: false),
                    Transferred = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billing_Billing_CreditorId",
                        column: x => x.CreditorId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Billing_Billing_DebtorId",
                        column: x => x.DebtorId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Billing_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Billing_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Billing_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Facilitator",
                schema: "Bill",
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
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Facilitator_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Financier",
                schema: "Bill",
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
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Financier_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Guarantor",
                schema: "Bill",
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
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guarantor_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                schema: "Bill",
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
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organization_Organization_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Bill",
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Organization_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantIpgSetting",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IpgType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantIpgSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantIpgSetting_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantMerchantContract",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    EnamadLink = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InternetBusinessLicenseLink = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    SettlementType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCommissionExchanged = table.Column<bool>(type: "bit", nullable: false),
                    InstallmentsCount = table.Column<int>(type: "int", nullable: true),
                    CommissionDeductionMethodType = table.Column<int>(type: "int", nullable: true),
                    InterestPercentage = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    InterestReferenceTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingPeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    BillingPeriod = table.Column<int>(type: "int", nullable: false),
                    DailyBillingOriginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillingBreak = table.Column<int>(type: "int", nullable: true),
                    PaymentMethodType = table.Column<byte>(type: "tinyint", nullable: false),
                    GuaranteeType = table.Column<byte>(type: "tinyint", nullable: true),
                    GuaranteeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CommissionCalculationType = table.Column<int>(type: "int", nullable: false),
                    TieredCommissions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FixedAmountCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    FixedPercentageCommission = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    CommissionReferenceTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TransactionMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMerchantContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantMerchantContract_Merchant_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "Bill",
                        principalTable: "Merchant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantMerchantContract_TenantMerchantContract_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Bill",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantMerchantContract_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillingPayment",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillingId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingPayment_Billing_BillingId",
                        column: x => x.BillingId,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MerchantBilling",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseTransactionsCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    RefundedTransactionsCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PurchaseTransactionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PurchaseTransactionsCalculatedCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    RefundedTransactionsAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBilling", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantBilling_Billing_Id",
                        column: x => x.Id,
                        principalSchema: "Bill",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContract",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FeeCalculationType = table.Column<byte>(type: "tinyint", nullable: false),
                    CommissionCalculationType = table.Column<int>(type: "int", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TieredCommissions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FixedAmountCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    FixedPercentageCommission = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    CommissionReferenceTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TransactionMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    BillingPeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    BillingPeriod = table.Column<int>(type: "int", nullable: false),
                    DailyBillingOriginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GracePeriod = table.Column<int>(type: "int", nullable: true),
                    PenaltyPercent = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    TenantIpgSettingId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContract_TenantIpgSetting_TenantIpgSettingId",
                        column: x => x.TenantIpgSettingId,
                        principalSchema: "Bill",
                        principalTable: "TenantIpgSetting",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantPlatformContract_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletContract",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    RootParentId = table.Column<int>(type: "int", nullable: true),
                    TenantIpgSettingId = table.Column<int>(type: "int", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContract_TenantIpgSetting_TenantIpgSettingId",
                        column: x => x.TenantIpgSettingId,
                        principalSchema: "Bill",
                        principalTable: "TenantIpgSetting",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletContract_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FinancialDocument",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    PaymentGatewayType = table.Column<byte>(type: "tinyint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MerchantBranchId = table.Column<int>(type: "int", nullable: true),
                    TenantMerchantContractId = table.Column<int>(type: "int", nullable: true),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: true),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RefundReason = table.Column<byte>(type: "tinyint", nullable: true),
                    RefundDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RefundType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreditAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CashAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PrepaymentAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialDocument_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocument_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocument_FinancialDocument_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Bill",
                        principalTable: "FinancialDocument",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocument_MerchantBranch_MerchantBranchId",
                        column: x => x.MerchantBranchId,
                        principalSchema: "Bill",
                        principalTable: "MerchantBranch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocument_TenantMerchantContract_TenantMerchantContractId",
                        column: x => x.TenantMerchantContractId,
                        principalSchema: "Bill",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocument_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Bill",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialDocument_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractFacilitator",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: false),
                    FixedAmountCommissionPercentage = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    TransactionsCommissionPercentage = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    PaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractFacilitator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractFacilitator_Facilitator_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalSchema: "Bill",
                        principalTable: "Facilitator",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractFacilitator_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Bill",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantPlatformContractProvider",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantPlatformContractId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlatformContractProvider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractProvider_Provider_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "Bill",
                        principalTable: "Provider",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantPlatformContractProvider_TenantPlatformContract_TenantPlatformContractId",
                        column: x => x.TenantPlatformContractId,
                        principalSchema: "Bill",
                        principalTable: "TenantPlatformContract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletContractFacilitator",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    FacilitatorId = table.Column<int>(type: "int", nullable: false),
                    PortionTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommissionCalculationType = table.Column<int>(type: "int", nullable: true),
                    FixedAmountCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    FixedPercentageCommission = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    TransactionMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TransactionMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TieredCommissions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractFacilitator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractFacilitator_Facilitator_FacilitatorId",
                        column: x => x.FacilitatorId,
                        principalSchema: "Bill",
                        principalTable: "Facilitator",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletContractFacilitator_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Bill",
                        principalTable: "WalletContract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletContractFinancier",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    FinancierId = table.Column<int>(type: "int", nullable: false),
                    PortionTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommissionCalculationType = table.Column<int>(type: "int", nullable: true),
                    FixedAmountCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    FixedPercentageCommission = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    TransactionMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TransactionMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TieredCommissions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractFinancier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractFinancier_Financier_FinancierId",
                        column: x => x.FinancierId,
                        principalSchema: "Bill",
                        principalTable: "Financier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletContractFinancier_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Bill",
                        principalTable: "WalletContract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletContractGuarantor",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    WalletContractId = table.Column<int>(type: "int", nullable: false),
                    GuarantorId = table.Column<int>(type: "int", nullable: false),
                    PortionTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommissionCalculationType = table.Column<int>(type: "int", nullable: true),
                    FixedAmountCommission = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    FixedPercentageCommission = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    TransactionMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TransactionMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMinCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    PeriodMaxCommissionAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: true),
                    TieredCommissions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PaymentMethodType = table.Column<byte>(type: "tinyint", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletContractGuarantor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletContractGuarantor_Guarantor_GuarantorId",
                        column: x => x.GuarantorId,
                        principalSchema: "Bill",
                        principalTable: "Guarantor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletContractGuarantor_WalletContract_WalletContractId",
                        column: x => x.WalletContractId,
                        principalSchema: "Bill",
                        principalTable: "WalletContract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Installment",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FinancialDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    FromBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    ToBusinessIdentityId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CheckSum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CashAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false),
                    PrepaymentAmount = table.Column<decimal>(type: "decimal(32,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installment_BusinessIdentity_FromBusinessIdentityId",
                        column: x => x.FromBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Installment_BusinessIdentity_ToBusinessIdentityId",
                        column: x => x.ToBusinessIdentityId,
                        principalSchema: "Bill",
                        principalTable: "BusinessIdentity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Installment_FinancialDocument_FinancialDocumentId",
                        column: x => x.FinancialDocumentId,
                        principalSchema: "Bill",
                        principalTable: "FinancialDocument",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Installment_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Bill",
                        principalTable: "Tenant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MerchantInstallment",
                schema: "Bill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TenantMerchantContractId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantInstallment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantInstallment_Installment_Id",
                        column: x => x.Id,
                        principalSchema: "Bill",
                        principalTable: "Installment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantInstallment_TenantMerchantContract_TenantMerchantContractId",
                        column: x => x.TenantMerchantContractId,
                        principalSchema: "Bill",
                        principalTable: "TenantMerchantContract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_JobId",
                schema: "Bill",
                table: "BackgroundJob",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billing_CreditorId",
                schema: "Bill",
                table: "Billing",
                column: "CreditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_DebtorId",
                schema: "Bill",
                table: "Billing",
                column: "DebtorId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_FromBusinessIdentityId",
                schema: "Bill",
                table: "Billing",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_TenantId",
                schema: "Bill",
                table: "Billing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_ToBusinessIdentityId",
                schema: "Bill",
                table: "Billing",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingPayment_BillingId",
                schema: "Bill",
                table: "BillingPayment",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_Facilitator_TenantId",
                schema: "Bill",
                table: "Facilitator",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_FromBusinessIdentityId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_MerchantBranchId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "MerchantBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_ParentId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_TenantId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_TenantMerchantContractId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "TenantMerchantContractId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_TenantPlatformContractId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "TenantPlatformContractId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialDocument_ToBusinessIdentityId",
                schema: "Bill",
                table: "FinancialDocument",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Financier_TenantId",
                schema: "Bill",
                table: "Financier",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Guarantor_TenantId",
                schema: "Bill",
                table: "Guarantor",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_FinancialDocumentId",
                schema: "Bill",
                table: "Installment",
                column: "FinancialDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_FromBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                column: "FromBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_TenantId",
                schema: "Bill",
                table: "Installment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Installment_ToBusinessIdentityId",
                schema: "Bill",
                table: "Installment",
                column: "ToBusinessIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantBranch_MerchantId",
                schema: "Bill",
                table: "MerchantBranch",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantInstallment_TenantMerchantContractId",
                schema: "Bill",
                table: "MerchantInstallment",
                column: "TenantMerchantContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ParentId",
                schema: "Bill",
                table: "Organization",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_TenantId",
                schema: "Bill",
                table: "Organization",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantIpgSetting_TenantId",
                schema: "Bill",
                table: "TenantIpgSetting",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContract_MerchantId",
                schema: "Bill",
                table: "TenantMerchantContract",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContract_ParentId",
                schema: "Bill",
                table: "TenantMerchantContract",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMerchantContract_TenantId",
                schema: "Bill",
                table: "TenantMerchantContract",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContract_TenantId",
                schema: "Bill",
                table: "TenantPlatformContract",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContract_TenantIpgSettingId",
                schema: "Bill",
                table: "TenantPlatformContract",
                column: "TenantIpgSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractFacilitator_FacilitatorId",
                schema: "Bill",
                table: "TenantPlatformContractFacilitator",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractFacilitator_TenantPlatformContractId",
                schema: "Bill",
                table: "TenantPlatformContractFacilitator",
                column: "TenantPlatformContractId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractProvider_ProviderId",
                schema: "Bill",
                table: "TenantPlatformContractProvider",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlatformContractProvider_TenantPlatformContractId",
                schema: "Bill",
                table: "TenantPlatformContractProvider",
                column: "TenantPlatformContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_TenantId",
                schema: "Bill",
                table: "WalletContract",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContract_TenantIpgSettingId",
                schema: "Bill",
                table: "WalletContract",
                column: "TenantIpgSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFacilitator_FacilitatorId",
                schema: "Bill",
                table: "WalletContractFacilitator",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFacilitator_WalletContractId",
                schema: "Bill",
                table: "WalletContractFacilitator",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFinancier_FinancierId",
                schema: "Bill",
                table: "WalletContractFinancier",
                column: "FinancierId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractFinancier_WalletContractId",
                schema: "Bill",
                table: "WalletContractFinancier",
                column: "WalletContractId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractGuarantor_GuarantorId",
                schema: "Bill",
                table: "WalletContractGuarantor",
                column: "GuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletContractGuarantor_WalletContractId",
                schema: "Bill",
                table: "WalletContractGuarantor",
                column: "WalletContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "BillingPayment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "MerchantBilling",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "MerchantInstallment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Organization",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Outbox",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "TenantPlatformContractFacilitator",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "TenantPlatformContractProvider",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "WalletContractFacilitator",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "WalletContractFinancier",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "WalletContractGuarantor",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Billing",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Installment",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Provider",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Facilitator",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Financier",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Guarantor",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "WalletContract",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "FinancialDocument",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "MerchantBranch",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "TenantMerchantContract",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "TenantPlatformContract",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Merchant",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "TenantIpgSetting",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "Bill");

            migrationBuilder.DropTable(
                name: "BusinessIdentity",
                schema: "Bill");
        }
    }
}
