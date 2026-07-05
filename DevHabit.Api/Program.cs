using DevHabit.Api;
using DevHabit.Api.Extensions;
using DevHabit.Api.Middleware;
using DevHabit.Api.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationServices()
    .AddBackgroundJobs()
    .AddCorsPolicy()
    .AddRateLimiting();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();

    //使用外部Swagger替代内置的openapi工具
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference(opt =>
    {
        opt.WithOpenApiRoutePattern("/swagger/1.0/swagger.json");//若使用内置的OpenApi工具，则不用此配置。使用Swagger需要配置
    });

    await app.ApplyMigrationsAsync();//在生产环境中不使用此方法进行数据库迁移
    //但在集成测试中需要此方法！！！

    await app.SeedInitialDataAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors(CorsOptions.PolicyName);

//app.UseResponseCaching();//仅对未认证请求的轻量缓存

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();//速率限制器可放在管道的不同位置，依实际情况添加。
//仍需在控制器中添加EnableRateLimiting属性

app.UseMiddleware<ETagMiddleware>();

app.MapControllers();

await app.RunAsync();

//public partial class Program;若集成测试未能正确引用Program，启用此行