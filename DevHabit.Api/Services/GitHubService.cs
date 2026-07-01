using DevHabit.Api.DTOs.GitHub;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DevHabit.Api.Services;

public sealed class GitHubService(IHttpClientFactory httpClientFactory, ILogger<GitHubService> logger)
{
    public async Task<GitHubUserProfileDto?> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var client = CreateGitHubClient(accessToken);

        var response = await client.GetAsync("user", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("获取GitHub用户信息失败。状态码：{StatusCode}", response.StatusCode);
            return null;
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<GitHubUserProfileDto>(content);
    }

    public async Task<IReadOnlyList<GitHubEventDto>?> GetUserEventsAsync(string username, string accessToken, int page, int perPage, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);

        using var client = CreateGitHubClient(accessToken);

        var response = await client.GetAsync($"users/{username}/events?page={page}&per_page={perPage}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("获取GitHub用户事件失败。状态码：{StatusCode}", response.StatusCode);
            return null;
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<GitHubEventDto>>(content);
    }

    private HttpClient CreateGitHubClient(string accessToken)
    {
        var client = httpClientFactory.CreateClient("github");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}