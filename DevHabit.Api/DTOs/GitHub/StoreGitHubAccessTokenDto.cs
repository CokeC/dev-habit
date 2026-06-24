using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DevHabit.Api.DTOs.GitHub;

public sealed record StoreGitHubAccessTokenDto
{
    [NotNull]
    public string? AccessToken { get; set; }
    public int? ExpiresInDays { get; set; }
}