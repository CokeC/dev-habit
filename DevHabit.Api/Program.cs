using DevHabit.Api;
using DevHabit.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();//在生产环境中不使用此方法进行数据库迁移
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

await app.RunAsync();
