using Microsoft.Extensions.Configuration;
using Rest.Integration.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rest.Integration.Tests.Base
{
    public interface IAccessTokenManager
    {
        Task<string> GetAccessToken();
    }
    public class AccessTokenManager : IAccessTokenManager
    {
        private readonly IConfiguration _configuration;

        public AccessTokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetAccessToken()
        {

            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) })
            {
                var baseUrl = _configuration["AccessTokenInfo:BaseUrl"];

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
                ["client_id"] = _configuration["AccessTokenInfo:ClientId"]!,
                ["client_secret"] = _configuration["AccessTokenInfo:ClientSecret"]!
                
            };

            return new FormUrlEncodedContent(parameters);
        }
    }
}
