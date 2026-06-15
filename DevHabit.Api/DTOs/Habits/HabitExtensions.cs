using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

public static class HabitExtensions
{
    public static HabitDto ToHabitDto(this Habit habit)
    {
        return new()
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new()
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            Target = new()
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit,
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            Milestone = habit.Milestone == null ? null : new()
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current,
            },
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }

    public static HabitWithTagsDto ToHabitWithTagsDto(this Habit habit)
    {
        return new()
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new()
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            Target = new()
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit,
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            Milestone = habit.Milestone == null ? null : new()
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current,
            },
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc,
            Tags = [.. habit.Tags.Select(e => e.Name)]
        };
    }

    public static HabitWithTagsDtoV2 ToHabitWithTagsDtoV2(this Habit habit)
    {
        return new()
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new()
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            Target = new()
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit,
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            Milestone = habit.Milestone == null ? null : new()
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current,
            },
            CreatedAt = habit.CreatedAtUtc,
            UpdatedAt = habit.UpdatedAtUtc,
            LastCompletedAt = habit.LastCompletedAtUtc,
            Tags = [.. habit.Tags.Select(e => e.Name)]
        };
    }
}
