using DevHabit.Api.Collections;
using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.DTOs.Entrys;

public record EntriesQueryParameters : AcceptHeaderDto
{
    public string? Sort { get; set; }
    public string? Fields { get; set; }
    public string? HabitId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public bool IsArchived { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}