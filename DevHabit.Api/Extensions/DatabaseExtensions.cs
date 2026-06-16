using DevHabit.Api.Database;
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
}
