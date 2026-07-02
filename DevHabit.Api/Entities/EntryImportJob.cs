using DevHabit.Api.Collections;

namespace DevHabit.Api.Entities;

public  class EntryImportJob
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public EntryImportStatus Status { get; set; }
    public required string FileName { get; set; }
    public required byte[] FileContent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime CompleteAtUtc { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public List<string> Errors { get; set; } = [];
}