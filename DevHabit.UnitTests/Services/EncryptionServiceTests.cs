
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace DevHabit.UnitTests.Services;

public class EncryptionServiceTests
{
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        var options = Options.Create(new EncryptionOptions
        {
            Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        });

        _encryptionService = new EncryptionService(options);
    }

    [Fact]
    public void Decrypt_ShouldReturnPlainText_CorrectCiphertext()
    {
        const string plainText = "sensitive data";
        var ciphertext = _encryptionService.Encrypt(plainText);

        var decryptedCiphertext = _encryptionService.Decrypt(ciphertext);

        Assert.Equal(plainText, decryptedCiphertext);
    }
}
