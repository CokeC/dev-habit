using System.Collections.ObjectModel;

namespace DevHabit.Api.Entities;

public sealed class Habit
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitType Type { get; set; }

    public Frequency Frequency { get; set; } = new();
    public Target Target { get; set; } = new();
    public HabitStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? EndDate { get; set; }
    public Milestone? Milestone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastCompletedAtUtc { get; set; }

    public List<HabitTag> HabitTags { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
}

public enum HabitType
{
    None, Binary, Measurable
}

public enum HabitStatus
{
    None, Ongoing, Completed
}

public sealed class Frequency
{
    public FrequencyType Type { get; set; }
    public int TimesPerPeriod { get; set; }
}

public enum FrequencyType
{
    None, Daily, Weekly, Monthly
}

public sealed class Target
{
    public int Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public sealed class Milestone
{
    public int Target { get; set; }
    public int Current { get; set; }
}