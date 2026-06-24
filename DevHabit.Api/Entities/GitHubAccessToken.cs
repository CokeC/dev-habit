using System.Diagnostics.CodeAnalysis;

namespace DevHabit.Api.Entities;

public sealed class GitHubAccessToken
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    [NotNull]
    public string? Token { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}