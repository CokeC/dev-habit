using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DevHabit.Api.Jobs;

public sealed class CleanupEntryImportJobsJob(ApplicationDbContext dbContext, ILogger<CleanupEntryImportJobsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            //删除超过7天的成功任务
            var completedJobsCutoffDate = DateTime.UtcNow.AddDays(-7);
            var deletedCount = await dbContext.EntryImportJobs
                .Where(e => e.Status == EntryImportStatus.Complete)
                .Where(e => e.CompleteAtUtc < completedJobsCutoffDate)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
                logger.LogInformation("删除了{Count}个导入任务", deletedCount);

            //删除超过30天的失败任务
            var failedJobsCutoffDate = DateTime.UtcNow.AddDays(-30);

            deletedCount = await dbContext.EntryImportJobs
                .Where(e => e.Status == EntryImportStatus.Failed)
                .Where(e => e.CompleteAtUtc < failedJobsCutoffDate)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
                logger.LogInformation("删除了{Count}个失败导入任务", deletedCount);

            //删除超过2小时的卡住的任务
            var processingJobsCutoffDate = DateTime.UtcNow.AddHours(-2);

            deletedCount = await dbContext.EntryImportJobs
                .Where(e => e.Status == EntryImportStatus.Processing)
                .Where(e => e.CompleteAtUtc < processingJobsCutoffDate)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
                logger.LogInformation("删除了{Count}个卡住的导入任务", deletedCount);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "删除任务失败");
        }
    }
}
