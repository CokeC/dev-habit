using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using var idDbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("应用程序数据库迁移成功！");

            await idDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("身份数据库迁移成功！");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "数据库迁移发生错误！");
            throw;
        }
    }

    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            if (!await roleManager.RoleExistsAsync(Roles.Member))
                await roleManager.CreateAsync(new(Roles.Member));
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new(Roles.Admin));
            app.Logger.LogInformation("角色创建成功！");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "角色创建失败！");
            throw;
        }
    }
}