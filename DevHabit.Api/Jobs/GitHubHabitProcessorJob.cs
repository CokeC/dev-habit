using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DevHabit.Api.Jobs;

[DisallowConcurrentExecution]
public sealed class GitHubHabitProcessorJob(
    ApplicationDbContext dbContext,
    GitHubAccessTokenService gitHubAccessTokenService,
    //GitHubService gitHubService,
    RefitGitHubService gitHubService,
    ILogger<GitHubHabitProcessorJob> logger) : IJob
{
    private const string PushEventType = "PushEvent";

    public async Task Execute(IJobExecutionContext context)
    {
        var habitId = context.JobDetail.JobDataMap.GetString("habitId")
            ?? throw new InvalidOperationException("未找到关于habitId的任务数据！");

        try
        {
            logger.LogInformation("开始关于{HabitId}的GitHub事件", habitId);

            var habit = await dbContext.Habits
                .FirstOrDefaultAsync(e => e.Id == habitId && e.AutomationSource == AutomationSource.GitHub && !e.IsArchived, context.CancellationToken);

            if (habit is null)
            {
                logger.LogWarning("未找到{HabitId}号习惯，或未配置GitHub自动！", habitId);
                return;
            }

            var accessToken = await gitHubAccessTokenService.GetAsync(habit.UserId, context.CancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                logger.LogWarning("未找到{UserId}用户的GitHub连接密钥！", habit.UserId);
                return;
            }

            var profile = await gitHubService.GetUserProfileAsync(accessToken, context.CancellationToken);

            if(profile is null || profile.Login is null)
            {
                logger.LogWarning("无法获得{UserId}用户的GitHub资料！", habit.UserId);
                return;
            }

            var gitHubEvents = new List<GitHubEventDto>();
            const int perPage = 100;
            const int pagesToFetch = 10;

            for (int page = 1; page <= pagesToFetch; page++)
            {
                var pageEvents = await gitHubService.GetUserEventsAsync(
                    profile.Login,
                    accessToken,
                    page,
                    perPage,
                    context.CancellationToken);

                if (pageEvents is null || !pageEvents.Any())
                    break;

                gitHubEvents.AddRange(pageEvents);
            }

            if (!gitHubEvents.Any())
            {
                logger.LogWarning("无法获得{UserId}用户的GitHub事件！", habit.UserId);
                return;
            }

            var pushEvents = gitHubEvents.Where(e => e.Type == PushEventType).ToList();

            logger.LogInformation("发现关于{HabitId}习惯的{Count}条推送事件！", habitId, pushEvents.Count);

            foreach(var gitHubEventDto in pushEvents)
            {
                var exists = await dbContext.Entries.AnyAsync(
                    e => e.HabitId == habitId && e.ExternalId == gitHubEventDto.Id, context.CancellationToken);
                if (exists)
                {
                    logger.LogDebug("查找到事件{EventId}的条目", gitHubEventDto.Id);
                    continue;
                }

                var entry = new Entry
                {
                    Id = $"e_{Guid.CreateVersion7()}",
                    HabitId = habit.Id,
                    UserId = habit.UserId,
                    Value = 1,//每次推送1条
                    Notes = $"""
                    {gitHubEventDto.Actor.Login} pushed:

                    {string.Join(Environment.NewLine, gitHubEventDto.Payload.Commits?.Select(e => $"- {e.Message}") ?? [])}
                    """,
                    Date = DateOnly.FromDateTime(gitHubEventDto.CreatedAt),
                    Source = EntrySource.Automation,
                    ExternalId = gitHubEventDto.Id,
                    CreatedAtUtc = DateTime.UtcNow
                };

                dbContext.Entries.Add(entry);
                logger.LogInformation("为{Habit}号习惯创建事件{EventId}", habitId, gitHubEventDto.Id);
            }
            await dbContext.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("完成关于{HabitId}的GitHub事件", habitId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "为{Habit}号习惯创建GitHub事件时发生错误！", habitId);
            throw;
        }
    }
}
