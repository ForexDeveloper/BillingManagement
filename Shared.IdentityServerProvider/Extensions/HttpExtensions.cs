namespace Shared.IdentityServerProvider.Extensions;

internal static class HttpExtensions
{
    public static HttpClient CreateHttpClientByKey(this IHttpClientFactory httpClientFactory, string key, TimeSpan Timeout)
    {
        var httpClient = httpClientFactory.CreateClient(key);
        httpClient.Timeout = Timeout;
        return httpClient;
    }
}

