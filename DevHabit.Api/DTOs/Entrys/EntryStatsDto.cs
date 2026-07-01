namespace DevHabit.Api.DTOs.Entrys;

public class EntryStatsDto
{
    public List<DailyStatsDto> DailyStats { get; set; } = [];
    public int TotalEntries { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
}