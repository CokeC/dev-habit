using DevHabit.Api.Collections;

namespace DevHabit.Api.DTOs.Entrys;

public class EntryQueryParameters
{
    public string? Sort { get; set; }
    public string? Fields { get; set; }
    public string? HabitId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public bool? IsArchived { get; set; }
    public required int Page { get; set; } = 1;
    public required int PageSize { get; set; } = 10;
    public required bool IncludeLinks { get; set; }
}