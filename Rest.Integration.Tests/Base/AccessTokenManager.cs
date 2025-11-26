using System.Text.Json;
using Rest.Integration.Tests.Models;
using Microsoft.Extensions.Configuration;

namespace Rest.Integration.Tests.Base;

public interface IAccessTokenManager
{
    Task<string> GetAccessToken();
}

public class AccessTokenManager(IConfiguration configuration) : IAccessTokenManager
{
    public async Task<string> GetAccessToken()
    {
        using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) })
        {
            var baseUrl = configuration["AccessTokenInfo:BaseUrl"];

            httpClient.BaseAddress = new Uri(baseUrl!);

            var content = CreateTokenRequestContent();

            var response = await httpClient.PostAsync("connect/token", content);

            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonSerializer.Deserialize<LoginDto>(result);

            return tokenResponse!.AccessToken;
        }
    }

    private FormUrlEncodedContent CreateTokenRequestContent()
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = configuration["AccessTokenInfo:ClientId"]!,
            ["client_secret"] = configuration["AccessTokenInfo:ClientSecret"]!

        };

        return new FormUrlEncodedContent(parameters);
    }
}