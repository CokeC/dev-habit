namespace DevHabit.Api.Entities;

public sealed class HabitTag
{
    public string HabitId { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
