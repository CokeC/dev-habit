namespace DevHabit.Api.DTOs.GitHub;

public sealed record StoreGitHubAccessTokenDto
{
    public string? AccessToken { get; set; }
    public int? ExpiresInDays { get; set; }
}