using DevHabit.Api.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace DevHabit.Api.Services;

public sealed class EncryptionService(IOptions<EncryptionOptions> options)
{
    private readonly byte[] _masterKey = Convert.FromBase64String(options.Value.Key);
    private const int IvSize = 16;

    public string Encrypt(string plainText)
    {

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _masterKey;
        aes.IV = RandomNumberGenerator.GetBytes(IvSize);

        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, IvSize);

        using (ICryptoTransform cryptoTransform = aes.CreateEncryptor())
        using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plainText);
        }
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var cipherData = Convert.FromBase64String(cipherText);

        if (cipherData.Length < IvSize)
            throw new InvalidDataException("密文格式错误！");

        var iv = new byte[IvSize];
        var encryptedData = new byte[cipherData.Length - IvSize];

        Buffer.BlockCopy(cipherData, 0, iv, 0, IvSize);
        Buffer.BlockCopy(cipherData, IvSize, encryptedData, 0, encryptedData.Length);

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _masterKey;
        aes.IV = iv;

        using var memoryStream = new MemoryStream(encryptedData);
        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }
}
