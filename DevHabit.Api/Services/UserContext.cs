using DevHabit.Api.Database;
using DevHabit.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.Api.Services;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext, IMemoryCache memoryCache)
{
    private const string CacheKeyPrefix = "users:id:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var identityId = httpContextAccessor.HttpContext?.User.GetIdentityId();
        if (identityId is null)
            return null;

        var cacheKey = $"{CacheKeyPrefix}{identityId}";

        var userId = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);

            var userId = await dbContext.Users
                .Where(e => e.IdentityId == identityId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken);
            return userId;
        });

        return userId;
    }
}
