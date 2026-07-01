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

public record EntriesCursorQueryParameters : AcceptHeaderDto
{
    public string? Cursor { get; init; }
    public string? Fields { get; init; }
    public string? HabitId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public bool IsArchived { get; init; } = true;
    public int Limit { get; init; } = 10;
}