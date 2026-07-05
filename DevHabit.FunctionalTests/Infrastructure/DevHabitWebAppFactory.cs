using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Testcontainers.PostgreSql;
using WireMock.Server;

namespace DevHabit.FunctionalTests.Infrastructure;

public class DevHabitWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    //const string devHabitStorage = "devhabit.postgres";
    //const string connectionStrings = "Host=devhabit.postgres;Database=devhabit;Username=postgres;Password=postgres";

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("swr.cn-north-4.myhuaweicloud.com/ddn-k8s/docker.io/postgres:18.4")
        .WithDatabase("devhabit")
        .WithUsername("postgres")
        .WithPassword("postgres")
        //.WithHostname(devHabitStorage)
        //.WithExposedPort(5432)
        .Build();

    private WireMockServer? _wireMockServer;

    public WireMockServer GetWireMockServer() => _wireMockServer!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //使用下面的数据库连接字符串，以代替原来应用中的相同配置项
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString());

        builder.UseSetting("GitHub:BaseUrl", _wireMockServer!.Urls[0]);
        builder.UseSetting("Encryption:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        Quartz.Logging.LogContext.SetCurrentLogProvider(NullLoggerFactory.Instance);
    }


    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        //await _postgresContainer.ExecScriptAsync("script");//可以在此执行初始化数据库代码，以替代原来在program中执行的迁移、初始化代码


        _wireMockServer = WireMockServer.Start();
    }

    //因WebApplicationFactory已实现DisposeAsync()，要实现IAsyncLifetime的同名方法，必须加"new"
    public new async ValueTask DisposeAsync()
    {
        await _postgresContainer.StopAsync();

        _wireMockServer!.Stop();
    }
}
