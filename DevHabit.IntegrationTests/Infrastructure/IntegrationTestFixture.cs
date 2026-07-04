
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WireMock.Server;

namespace DevHabit.IntegrationTests.Infrastructure;


//[Collection(nameof(IntegrationTestCollection))]与相同特性的类共享一个Web实例
public abstract class IntegrationTestFixture(DevHabitWebAppFactory factory) : IClassFixture<DevHabitWebAppFactory>
{
    private HttpClient? _authorizedClient;

    public WireMockServer WireMockServer => factory.GetWireMockServer();
    public HttpClient CreateClient() => factory.CreateClient();

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email = "test@test.com", string password = "Test123!")
    {
        if (_authorizedClient is not null)
            return _authorizedClient;

        var client = CreateClient();

        bool userExists;
        using(var scope = factory.Services.CreateScope())
        {
            using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            userExists = await dbContext.Users.AnyAsync(x => x.Email == email);
        }

        if (!userExists)
        {
            var registerResponse = await client.PostAsJsonAsync(Routes.Auth.Register, new RegisterUserDto
            {
                Name = email,
                Email = email,
                Password = password,
                ConfirmPassword = password,

            });

            registerResponse.EnsureSuccessStatusCode();
        }

        var loginResponse = await client.PostAsJsonAsync(Routes.Auth.Login, new LoginUserDto
        {
            Email = email,
            Password = password
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokensDto>();

        if (loginResult?.AccessToken is null)
            throw new InvalidOperationException("获取token失败！");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        _authorizedClient = client;
        return client;
    }

    protected async Task CleanupDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (connectionString == null)
            throw new InvalidOperationException("数据库连接字符串未找到！");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            DO $$
            BEGIN
                -- Truncate application tables
                TRUNCATE TABLE dev_habit.entries CASCADE;
                TRUNCATE TABLE dev_habit.entry_import_jobs CASCADE;
                TRUNCATE TABLE dev_habit.tags CASCADE;
                TRUNCATE TABLE dev_habit.habits CASCADE;
                TRUNCATE TABLE dev_habit.users CASCADE;
            
                -- Truncate identity tables
                TRUNCATE TABLE identity.asp_net_users CASCADE;
                TRUNCATE TABLE identity.refresh_tokens CASCADE;
            ", connection);

        await command.ExecuteNonQueryAsync();
    }
}
