using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.DTOs.GitHub;

public class GitHubUserProfileDto
{
    public string? Login { get; set; }
    public Int64 Id { get; set; }
    public string? Name { get; set; }
    public string? Avatar_url { get; set; }
    public string? Bio { get; set; }
    public int Public_repos { get; set; }
    public List<LinkDto> Links { get; set; } = [];
}