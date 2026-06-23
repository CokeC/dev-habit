using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace DevHabit.Api.Entities;

public class RefreshToken
{
    public Guid Id { set; get; }
    public required string UserId { set; get; }
    public required string Token { set; get; }
    public required DateTime ExpiresAtUtc { set; get; }
    [NotNull]
    public IdentityUser? User { set; get; }
}
//添加迁移命令：Add-Migration Add_RefreshTokens -Context AppIdentityDbContext -o Migrations/Identity