using CsvHelper;
using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.EntryImports;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Globalization;

namespace DevHabit.Api.Jobs;

public sealed class ProcessEntryImportJob(ApplicationDbContext dbContext, ILogger<ProcessEntryImportJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var importJobId = context.MergedJobDataMap.GetString("importJobId");
        var importJob = await dbContext.EntryImportJobs.FirstOrDefaultAsync(e => e.Id == importJobId);

        if (importJob == null)
        {
            logger.LogError("未发现任务{ImportJobId}", importJobId);
            return;
        }

        try
        {
            importJob.Status = EntryImportStatus.Processing;
            await dbContext.SaveChangesAsync();

            using var memoryStream = new MemoryStream();
            using var reader = new StreamReader(memoryStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = await csv.GetRecordsAsync<CsvEntryRecord>().ToListAsync();

            importJob.TotalRecords = records.Count;
            await dbContext.SaveChangesAsync();

            foreach(var record in records)
            {
                try
                {
                    var habit = await dbContext.Habits
                        .FirstOrDefaultAsync(e => e.Id == record.HabitId && e.UserId == importJob.UserId);

                    if (habit is null)
                        throw new InvalidOperationException($"Habit with Id {record.HabitId} does not exist");

                    var entry = new Entry
                    {
                        Id = $"e_{Guid.CreateVersion7()}",
                        UserId = importJob.UserId,
                        HabitId = record.HabitId,
                        Value = habit.Target.Value,
                        Date = record.Date,
                        Notes = record.Notes,
                        Source = EntrySource.FileImport,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    dbContext.Entries.Add(entry);
                    importJob.SuccessfulRecords++;
                }
                catch(Exception ex)
                {
                    importJob.FailedRecords++;
                    importJob.Errors.Add($"存储记录时错误：{ex.Message}");
                    if(importJob.Errors.Count >= 100)
                    {
                        importJob.Errors.Add("太多错误，停止错误收集！");
                        break;
                    }
                }
                finally
                {
                    importJob.ProcessedRecords++;
                }

                if(importJob.ProcessedRecords % 100 == 0)
                {
                    await dbContext.SaveChangesAsync();
                }
            }

            importJob.Status = EntryImportStatus.Complete;
            importJob.CompleteAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "错误任务{ImportJobId}", importJobId);

            importJob.Status = EntryImportStatus.Failed;
            importJob.Errors.Add($"灾难错误：{ex.Message}");
            importJob.CompleteAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }
}
