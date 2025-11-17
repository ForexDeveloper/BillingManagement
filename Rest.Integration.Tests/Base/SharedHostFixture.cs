using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.TenantAggregate;
using IdentityModel;
using IdentityModel.Client;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Rest.Integration.Tests.Base;
using Rest.Integration.Tests.Models;
using Service.Rest;
using Shared.IdentityServerProvider.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using static MassTransit.MessageHeaders;


namespace IntegrationTest.Server;

public class SharedHostFixture : IDisposable
{
    private readonly IHost _host;
    private readonly TestServer _server;
    private readonly HttpClient _httpClient;
    private readonly HttpMessageHandler _httpMessageHandler;
    private readonly ILogger<SharedHostFixture> _logger;
    private readonly IAccessTokenManager _tokenManager;
    private bool _isExistDb;
    private ApplicationDbContext _mainContext;
    private readonly BaseTestDataBuilder _baseTestDataBuilder;
    private bool _disposed;
    private Tenant _tenant;

    public SharedHostFixture()
    {
        SetEnvironment("Local");
        _host = CreateAndStartHost();

        _logger = _host.Services.GetRequiredService<ILogger<SharedHostFixture>>();
        _server = _host.GetTestServer();
        _httpMessageHandler = _server.CreateHandler();
        _httpClient = _host.GetTestClient();
        _tokenManager = new AccessTokenManager(Configuration);

        InitializeDatabase();

        _baseTestDataBuilder = new BaseTestDataBuilder(this);

        SetupCompleteTestData();


        _logger.LogInformation("Billing Management test server started");
    }

    public JsonSerializerOptions SerializerOptions { get; } = CreateJsonSerializerOptions();
    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

    private IHost CreateAndStartHost()
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

    private void InitializeDatabase()
    {
        GenerateTestSqlConnectionString();

        _mainContext = _host.Services.GetRequiredService<ApplicationDbContext>();

        if (!_isExistDb)
            _mainContext.Database.EnsureCreated();

        _isExistDb = true;
    }

    private void GenerateTestSqlConnectionString()
    {
        var connection = Configuration.GetConnectionString("ApplicationDbConnection");
        if (string.IsNullOrEmpty(connection))
            throw new InvalidOperationException("ApplicationDbConnection connection string is not configured");

        var dbName = GenerateUniqueDbName();
        var newConnection = connection.Replace("$_DbName_DontChangeIt_Its_A_Token_$", dbName);

        Configuration["ConnectionStrings:ApplicationDbConnection"] = newConnection;
    }

    private static string GenerateUniqueDbName()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return $"BillingManagement_{timestamp}__IntegrationTest";
    }

    public async Task<HttpClient> GetAuthenticatedHttpClientAsync()
    {
        var token = await _tokenManager.GetAccessToken();
        SetAccessToken(token);
        return _httpClient;
    }

    public HttpClient CreateUnauthenticatedHttpClient() => new HttpClient(_httpMessageHandler);

    public Tenant Tenant => _tenant;


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

    public void SetupCompleteTestData()
    {

        _tenant = _baseTestDataBuilder.CreateTenant().Result;


    }
}
