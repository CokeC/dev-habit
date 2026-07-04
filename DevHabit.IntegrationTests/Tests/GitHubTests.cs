
using DevHabit.Api.DTOs.GitHub;
using DevHabit.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Json;
using System.Net.Mime;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace DevHabit.IntegrationTests.Tests;

public class GitHubTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    private const string TestAccessToken = "gho_test123456789";

    private static readonly GitHubUserProfileDto User = new()
    {
        Login = "testuser",
        Name = "Test",
        Avatar_url = "https://github.com/testuser.png",
        Bio = "Test bio",
        Public_repos = 10
    };
    /*
    private static readonly GitHubEventDto TestEvent = new()
    {
        Id = "123456789",
        Type = "PushEvent",
        Actor = new()
        {
            Login = "abcd"
        },
        Payload = new()
        {
            Commits = []
        },
        CreatedAt = DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture)
    };*/

    [Fact]
    public async Task GetProfile_ShouldReturnUserProfile()
    {
        WireMockServer.Given(Request.Create()
            .WithPath("/user")
            .WithHeader("Authorization", $"Bearer {TestAccessToken}")
            .UsingGet())
            .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", MediaTypeNames.Application.Json)
            .WithBodyAsJson(User));

        var client = await CreateAuthenticatedClientAsync();

        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 3
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto, TestContext.Current.CancellationToken);

        //Act
        var response = await client.GetAsync(Routes.GitHub.GetProfile, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        //Assert
        var profile = JsonConvert.DeserializeObject<GitHubUserProfileDto>(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.NotNull(profile);
        Assert.Equivalent(User, profile);
    }
}