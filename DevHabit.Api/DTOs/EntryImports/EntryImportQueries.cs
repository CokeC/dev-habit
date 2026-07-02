using DevHabit.Api.DTOs.Entrys;
using DevHabit.Api.Entities;
using System.Linq.Expressions;
using static Quartz.Logging.OperationName;

namespace DevHabit.Api.DTOs.EntryImports;

public static class EntryImportQueries
{
    public static Expression<Func<EntryImportJob, EntryImportJobDto>> ProjectToDto()
    {
        return job => new()
        {
            Id = job.Id,
            UserId = job.UserId,
            Status = job.Status,
            FileName = job.FileName,
            FileContent = job.FileContent,
            CreateAtUtc = job.CreatedAtUtc
        };
    }
}
