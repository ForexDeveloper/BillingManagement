namespace Application.Service.Encryptions;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    bool Validate(string plainText, string encryptedText);
}
