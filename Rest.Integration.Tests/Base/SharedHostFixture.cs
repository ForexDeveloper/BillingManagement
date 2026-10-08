using Service.Rest;
using System.Text.Json;
using Domain.Core.Enums;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Microsoft.Extensions.DependencyInjection;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.Base;

public class SharedHostFixture : IDisposable
{
    private bool _disposed;
    private bool _isExistDb;
    private readonly IHost _host;
    private readonly TestServer _server;
    private readonly HttpClient _httpClient;
    private ApplicationDbContext _mainContext;
    private readonly IAccessTokenManager _tokenManager;
    private readonly ILogger<SharedHostFixture> _logger;
    private readonly HttpMessageHandler _httpMessageHandler;
    private readonly BaseTestDataBuilder _baseTestDataBuilder;
    private readonly SemaphoreSlim _syncDbCreationLock = new(1, 1);

    public SharedHostFixture()
    {
        SetEnvironment("Local");
        _host = CreateAndStartHost();
        _server = _host.GetTestServer();
        _httpClient = _host.GetTestClient();
        _httpMessageHandler = _server.CreateHandler();
        _tokenManager = new AccessTokenManager(Configuration);
        _logger = _host.Services.GetRequiredService<ILogger<SharedHostFixture>>();

        InitializeDatabaseAsync().GetAwaiter().GetResult();

        _baseTestDataBuilder = new BaseTestDataBuilder(this);

        SetupCompleteTestData();

        _logger.LogInformation("Billing Management test server started");
    }

    public JsonSerializerOptions SerializerOptions { get; } = CreateJsonSerializerOptions();

    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

    private static IHost CreateAndStartHost()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!)
            .ConfigureWebHost(ConfigureWebHost);

        return hostBuilder.Start();
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
    }

    private static void ConfigureWebHost(IWebHostBuilder webHost)
    {
        webHost.UseSetting("TestSection:Parameter", "Value")
               .UseTestServer()
               .UseStartup<Startup>();
    }

    private static void SetEnvironment(string environment)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", environment);
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private async Task InitializeDatabaseAsync()
    {
        if (_isExistDb) return;

        await _syncDbCreationLock.WaitAsync();

        try
        {
            if (_isExistDb) return;

            GenerateTestSqlConnectionString();

            _mainContext= _host.Services.GetRequiredService<ApplicationDbContext>();

             await _mainContext.Database.EnsureCreatedAsync();

            _isExistDb = true;
        }
        finally
        {
            _syncDbCreationLock.Release();
        }
    }

    private void GenerateTestSqlConnectionString()
    {
        var connection = Configuration.GetConnectionString("ApplicationDbConnection");

        if (string.IsNullOrEmpty(connection))
            throw new InvalidOperationException("ApplicationDbConnection connection string is not configured");

        var dbName = GenerateUniqueDbName();

        var newConnection = connection.Replace("$_DbName_DoNotChangeIt_Its_A_Token_$", dbName);

        Configuration["ConnectionStrings:ApplicationDbConnection"] = newConnection;
        Configuration["ConnectionStrings:ReadonlyDbConnection"] = newConnection;
    }

    private static string GenerateUniqueDbName()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return $"BillingManagement_IntegrationTest";
    }

    public async Task<HttpClient> GetAuthenticatedHttpClientAsync(int? tenantId = null, string? userId = null)
    {
        var token = await _tokenManager.GetAccessToken();

        SetAccessToken(token);

        if (!string.IsNullOrWhiteSpace(userId))
            _httpClient.DefaultRequestHeaders.Add("user_id", userId);

        if (tenantId != null)
            _httpClient.DefaultRequestHeaders.Add("tenant_id", tenantId.ToString());

        return _httpClient;
    }

    public HttpClient CreateUnauthenticatedHttpClient => new HttpClient(_httpMessageHandler);

    public Tenant Tenant { get; private set; }

    public Merchant Merchant { get; private set; }

    public TenantMerchantContract TenantMerchantContract { get; private set; }

    private void SetAccessToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public ApplicationDbContext GetMainContext() => _mainContext;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task FlushAsync()
    {
        var installments = _mainContext.Installments.AsQueryable();
        var billingPayments = _mainContext.BillingPayments.AsQueryable();
        var contracts = _mainContext.TenantMerchantContracts.AsQueryable();
        var financialDocuments = _mainContext.FinancialDocuments.AsQueryable();

        _mainContext.Installments.RemoveRange(installments);
        _mainContext.BillingPayments.RemoveRange(billingPayments);
        _mainContext.TenantMerchantContracts.RemoveRange(contracts);
        _mainContext.FinancialDocuments.RemoveRange(financialDocuments);

        await _mainContext.SaveChangesAsync();

        await _mainContext.Database.ExecuteSqlAsync($"DELETE Bill.Billing");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _logger.LogInformation("Payment test disposing resources");

            _httpMessageHandler?.Dispose();

            _httpClient?.Dispose();

            // You can comment this line to preserve the database for inspection
            _mainContext?.Database.EnsureDeleted();

            _server?.Dispose();
            _host?.Dispose();
        }

        _disposed = true;
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return (T)_host.Services.GetRequiredService(typeof(T));
    }

    private void SetupCompleteTestData()
    {
        Tenant = _baseTestDataBuilder.CreateTenant().Result;
        Merchant = _baseTestDataBuilder.CreateMerchant(Tenant.Id).Result;
        TenantMerchantContract = CreateTenantMerchantContract().Result;
    }

    public async Task<FinancialDocument> CreatePurchaseFinancialDocument(int contractId)
    {
        var FINANCIAL_DOCUMENT_ID = await GetUniqueFinancialDocumentId();

        var financialDocument = new FinancialDocument(FINANCIAL_DOCUMENT_ID,
            Tenant.Id,
            Merchant.Id,
            Tenant.Id,
            100000000,
            40000000,
            30000000,
            30000000,
            FinancialDocumentType.Purchase,
            FinancialDocumentState.Verified,
            PaymentGatewayType.Ipg,
            null,
            null,
            contractId
        );

        await _mainContext.FinancialDocuments.AddAsync(financialDocument);

        await _mainContext.SaveChangesAsync();

        return financialDocument;
    }

    public async Task<FinancialDocument> CreateRefundFinancialDocument(FinancialDocument purchaseDocument, bool isPartial = false)
    {
        var FINANCIAL_DOCUMENT_ID = await GetUniqueFinancialDocumentId();

        var amount = purchaseDocument.Amount;
        var cashAmount = purchaseDocument.CashAmount;
        var creditAmount = purchaseDocument.CreditAmount;
        var prePaymentAmount = purchaseDocument.PrepaymentAmount;

        if (isPartial)
        {
            amount /= 10;
            cashAmount /= 10;
            creditAmount /= 10;
            prePaymentAmount /= 10;
        }

        var financialDocument = new FinancialDocument(FINANCIAL_DOCUMENT_ID,
            Tenant.Id,
            Merchant.Id,
            Tenant.Id,
            amount,
            creditAmount,
            cashAmount,
            prePaymentAmount,
            FinancialDocumentType.Refund,
            FinancialDocumentState.Verified,
            PaymentGatewayType.Ipg,
            null,
            null,
            purchaseDocument.TenantMerchantContractId,
            null,
            null,
            null,
            null,
            purchaseDocument.Id
        );

        await _mainContext.FinancialDocuments.AddAsync(financialDocument);

        await _mainContext.SaveChangesAsync();

        return financialDocument;
    }

    public async Task<TenantMerchantContract> CreateTenantMerchantContract()
    {
        var contract = new TenantMerchantContract(Tenant.Id,
            Merchant.Id,
            $"TN-MR{Random.Shared.Next(1, 1000)}",
            DateTime.Now,
            DateTime.Now.AddMonths(6),
            SettlementType.Installments,
            true,
            13,
            CommissionDeductionMethodType.DeductEquallyFromInstallments,
            null,
            [
                InterestReferenceType.CashAmount, InterestReferenceType.CreditAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            17,
            DateTime.MinValue,
            0,
            PaymentMethodType.BankAccountDeposit,
            GuaranteeType.House,
            null,
            CommissionCalculationType.FixedAmount,
            45000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.InterestAmount, CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000
        );

        await _mainContext.TenantMerchantContracts.AddAsync(contract);

        await _mainContext.SaveChangesAsync();

        return contract;
    }

    public async Task<TenantMerchantContract> CloneTenantMerchantContract(TenantMerchantContract contract)
    {
        var cloneContract = new TenantMerchantContract(contract.TenantId,
            contract.MerchantId,
            contract.ContractNumber,
            contract.StartDate,
            contract.EndDate,
            contract.SettlementType,
            contract.IsCommissionExchanged,
            contract.InstallmentsCount,
            contract.CommissionDeductionMethodType,
            contract.InterestPercentage,
            contract.InterestReferenceTypes,
            contract.BillingPeriodType,
            contract.BillingPeriod,
            contract.DailyBillingOriginDate,
            contract.BillingBreak,
            contract.PaymentMethodType,
            contract.GuaranteeType,
            contract.GuaranteeDescription,
            contract.CommissionCalculationType,
            contract.FixedAmountCommission,
            contract.FixedPercentageCommission,
            contract.CommissionReferenceTypes,
            contract.TransactionMinCommissionAmount,
            contract.TransactionMaxCommissionAmount,
            contract.PeriodMinCommissionAmount,
            contract.PeriodMaxCommissionAmount
        );

        contract.SetStatus(false);

        cloneContract.SetParentId(contract.Id);

        await _mainContext.TenantMerchantContracts.AddAsync(cloneContract);

        await _mainContext.SaveChangesAsync();

        return cloneContract;
    }

    private async Task<long> GetUniqueFinancialDocumentId()
    {
        var FINANCIAL_DOCUMENT_ID = Random.Shared.Next(1, 100000);

        var found = await _mainContext.FinancialDocuments.AnyAsync(p => p.Id == FINANCIAL_DOCUMENT_ID);

        if (found)
        {
            await GetUniqueFinancialDocumentId();
        }

        return FINANCIAL_DOCUMENT_ID;
    }
}