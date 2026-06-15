using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;

namespace DevHabit.Api.DTOs.Habits;

public static class HabitMappings
{
    public static void UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;
        habit.EndDate = dto.EndDate;

        habit.Frequency = new()
        {
            Type = dto.Frequency.Type,
            TimesPerPeriod = dto.Frequency.TimesPerPeriod
        };

        habit.Target = new()
        {
            Value = dto.Target.Value,
            Unit = dto.Target.Unit
        };

        if (dto.Milestone != null)
        {
            habit.Milestone ??= new();
            habit.Milestone.Target = dto.Milestone.Target;
        }

        habit.UpdatedAtUtc = DateTime.UtcNow;
    }

    public static readonly SortMappingDefinition SortMapping = new()
    {
        Mappings = [
            new SortMapping(nameof(HabitDto.Name), nameof(Habit.Name)),
            new SortMapping(nameof(HabitDto.Description), nameof(Habit.Description)),
            new SortMapping(nameof(HabitDto.Type), nameof(Habit.Type)),
            new SortMapping($"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.Type)}", $"{nameof(Habit.Frequency)}.{nameof(Frequency.Type)}"),
            new SortMapping($"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.TimesPerPeriod)}", $"{nameof(Habit.Frequency)}.{nameof(Frequency.TimesPerPeriod)}"),
            new SortMapping($"{nameof(HabitDto.Target)}.{nameof(TargetDto.Value)}", $"{nameof(Habit.Target)}.{nameof(Target.Value)}"),
            new SortMapping($"{nameof(HabitDto.Target)}.{nameof(TargetDto.Unit)}", $"{nameof(Habit.Target)}.{nameof(Target.Unit)}"),
            new SortMapping(nameof(HabitDto.Status), nameof(Habit.Status)),
            new SortMapping(nameof(HabitDto.EndDate), nameof(Habit.EndDate)),
            new SortMapping(nameof(HabitDto.CreatedAtUtc), nameof(Habit.CreatedAtUtc)),
            new SortMapping(nameof(HabitDto.UpdatedAtUtc), nameof(Habit.UpdatedAtUtc)),
            new SortMapping(nameof(HabitDto.LastCompletedAtUtc), nameof(Habit.LastCompletedAtUtc))
            ]
    };
}