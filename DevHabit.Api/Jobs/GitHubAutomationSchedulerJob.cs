using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DevHabit.Api.Jobs;

[DisallowConcurrentExecution]
public sealed class GitHubAutomationSchedulerJob(ApplicationDbContext dbContext, ILogger<GitHubAutomationSchedulerJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("开始GitHub计划任务！");
            var habitsToProcess = await dbContext.Habits
                .Where(e => e.AutomationSource == AutomationSource.GitHub && !e.IsArchived).ToListAsync(context.CancellationToken);

            logger.LogInformation("发现{Count}个关于GitHub的habit", habitsToProcess.Count);

            foreach (var habit in habitsToProcess)
            {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"github-habit-{habit.Id}", "github-habits")
                    .StartNow()
                    .Build();


                var jobDetail = JobBuilder.Create<GitHubHabitProcessorJob>()
                    .WithIdentity($"github-habit-{habit.Id}", "github-habits")
                    .UsingJobData("habitId", habit.Id)
                    .Build();

                await context.Scheduler.ScheduleJob(jobDetail, trigger);
                logger.LogInformation("设定{HabitId}的计划进程", habit.Id);
            }

            logger.LogInformation("结束GitHub计划任务！");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "设定GitHub计划任务失败！");
            throw;
        }
    }
}
