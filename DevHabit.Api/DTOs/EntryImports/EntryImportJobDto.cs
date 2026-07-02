using DevHabit.Api.Collections;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.EntryImports;

public class EntryImportJobDto
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public EntryImportStatus Status { get; set; }
    public required string FileName { get; set; }
    public required byte[] FileContent { get; set; }
    public DateTime CreateAtUtc { get; set; }
    public LinkDto[] Links { get; set; } = [];
}

public static class EntryImportJobExtensions
{
    public static EntryImportJobDto ToDto(this EntryImportJob job)
    {
        return new()
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