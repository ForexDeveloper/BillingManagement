using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Core.Helper;

public static class HashHelper
{
    private static readonly string _key;

    static HashHelper()
    {
        var configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", false)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", true)
           .Build();
        _key = configuration["PublicAppConfiguration:EncryptionKey"].ToString();
    }

    public static string Hash(this string text)
    {
        return GenerateSHA256Hash(text + _key);
    }

    public static bool Validate(this string text, string hashedText)
    {
        string hash = GenerateSHA256Hash(text + _key);
        return hash.Equals(hashedText, StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateSHA256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
