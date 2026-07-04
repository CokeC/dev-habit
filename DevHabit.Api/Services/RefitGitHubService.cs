using DevHabit.Api.DTOs.GitHub;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DevHabit.Api.Services;

public sealed class RefitGitHubService(IGitHubApi gitHubApi, ILogger<RefitGitHubService> logger)
{
    public async Task<GitHubUserProfileDto?> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessToken);

        var response = await gitHubApi.GetUserProfile("user", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("获取GitHub用户信息失败。状态码：{StatusCode}", response.StatusCode);
            return null;
        }
        return response.Content;
    }

    public async Task<IReadOnlyList<GitHubEventDto>?> GetUserEventsAsync(string username, string accessToken, int page = 1, int perPage = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);

        ArgumentException.ThrowIfNullOrEmpty(accessToken);

        var response = await gitHubApi.GetUserEvents(username, accessToken, page, perPage, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("获取GitHub用户事件失败。状态码：{StatusCode}", response.StatusCode);
            return null;
        }
        return response.Content;
    }
}