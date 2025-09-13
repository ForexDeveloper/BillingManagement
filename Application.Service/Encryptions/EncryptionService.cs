using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Application.Service.Encryptions;

public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly string _encryptionKey;

    public EncryptionService(IConfiguration configuration)
    {
        _configuration = configuration;
        _encryptionKey = _configuration["PublicAppConfiguration:EncryptionKey"]?.ToLower() ?? throw new Exception("EncryptionKey is not set in appsettings");
    }

    public string Encrypt(string plainText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = DeriveKey(_encryptionKey);
        aesAlg.GenerateIV();

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var msEncrypt = new MemoryStream();
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private string Decrypt(string encryptedText)
    {
        var fullCipher = Convert.FromBase64String(encryptedText);
        using var aesAlg = Aes.Create();
        aesAlg.Key = DeriveKey(_encryptionKey);

        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aesAlg.IV = iv;

        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    public bool Validate(string plainText, string encryptedText)
    {
        string decryptedText = Decrypt(encryptedText);
        return decryptedText.Equals(plainText, StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] DeriveKey(string privateKey)
    {
        var emptySalt = Array.Empty<byte>();
        var iterations = 1000;
        var desiredKeyLength = 16;
        var hashMethod = HashAlgorithmName.SHA384;
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(privateKey),
            emptySalt, iterations, hashMethod, desiredKeyLength);
    }
}
