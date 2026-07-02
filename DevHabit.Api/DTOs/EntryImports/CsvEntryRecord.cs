namespace DevHabit.Api.DTOs.EntryImports;

public class CsvEntryRecord
{
    public required string HabitId { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
}