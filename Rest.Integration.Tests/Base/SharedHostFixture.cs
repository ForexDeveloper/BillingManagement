using Service.Rest;
using System.Text.Json;
using Domain.Core.Enums;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.TestHost;
using Application.Service.Contracts;
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
    private readonly IMerchantInstallmentService _merchantInstallmentService;

    public SharedHostFixture()
    {
        SetEnvironment("Local");
        _host = CreateAndStartHost();
        _server = _host.GetTestServer();
        _httpClient = _host.GetTestClient();
        _httpMessageHandler = _server.CreateHandler();
        _tokenManager = new AccessTokenManager(Configuration);
        _logger = _host.Services.GetRequiredService<ILogger<SharedHostFixture>>();
        _merchantInstallmentService = _host.Services.GetRequiredService<IMerchantInstallmentService>();

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

            _mainContext ??= _host.Services.GetRequiredService<ApplicationDbContext>();

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
        return $"BillingManagement_{timestamp}__IntegrationTest";
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

    private void SetAccessToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public ApplicationDbContext GetMainContext() => _mainContext;

    public IMerchantInstallmentService GetMerchantInstallmentService() => _merchantInstallmentService;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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
    }

    public async Task<TenantMerchantContract> CreateTenantMerchantContract()
    {
        var tenantMerchantContract = new TenantMerchantContract(Tenant.Id,
            Merchant.Id,
            $"TN-MR{Random.Shared.Next(1, 10000000)}",
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
            DateTime.Today.AddDays(-100),
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

        await _mainContext.TenantMerchantContracts.AddAsync(tenantMerchantContract);

        await _mainContext.SaveChangesAsync();

        return tenantMerchantContract;
    }

    public async Task<FinancialDocument> CreateFinancialDocument(TenantMerchantContract contract)
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
            contract.Id
        );

        await _mainContext.FinancialDocuments.AddAsync(financialDocument);

        await _mainContext.SaveChangesAsync();

        return financialDocument;
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