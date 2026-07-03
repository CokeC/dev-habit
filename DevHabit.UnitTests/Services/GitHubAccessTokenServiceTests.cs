
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace DevHabit.UnitTests.Services;

public class GitHubAccessTokenServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EncryptionService _encryptionService;
    private readonly GitHubAccessTokenService _gitHubAccessTokenService;
    
    public GitHubAccessTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        _context = new ApplicationDbContext(options);

        var encryptionOptions = Options.Create(new EncryptionOptions
        {
            Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        });

        _encryptionService = new EncryptionService(encryptionOptions);
        _gitHubAccessTokenService = new GitHubAccessTokenService(_context, _encryptionService);
    }

    [Fact]
    public async Task StoreAsync_ShouldCreateNewToken()
    {
        const string userId = "user123";
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = "gitHub_token",
            ExpiresInDays = 30
        };

        await _gitHubAccessTokenService.StoreAsync(userId, dto, CancellationToken.None);

        var token = await _context.GitHubAccessTokens.SingleOrDefaultAsync(CancellationToken.None);
        Assert.NotNull(token);
        Assert.Equal(userId, token.UserId);
        Assert.NotEqual(dto.AccessToken, token.Token);
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
