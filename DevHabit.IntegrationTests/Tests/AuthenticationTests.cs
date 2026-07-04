
using DevHabit.Api.DTOs.Auth;
using DevHabit.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace DevHabit.IntegrationTests.Tests;

public class AuthenticationTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task Register_ShouldSucceed()
    {
        var dto = new RegisterUserDto
        {
            Name = "register@test.com",
            Email = "register@test.com",
            Password = "Password!123",
            ConfirmPassword = "password!123"
        };
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(Routes.Auth.Register, dto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturnTokens()
    {
        var dto = new RegisterUserDto
        {
            Name = "register@qq.com",
            Email = "register@qq.com",//如果名称与上个方法相同，会有并发错误
            Password = "Password!123",
            ConfirmPassword = "password!123"
        };
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(Routes.Auth.Register, dto, CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var accessTokensDto = await response.Content.ReadFromJsonAsync<AccessTokensDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(accessTokensDto);
    }
}
