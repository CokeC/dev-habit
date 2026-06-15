using System.Collections.ObjectModel;

namespace DevHabit.Api.DTOs.HabitTags;

public sealed record UpsertHabitTagsDto
{
    public required Collection<string> TagIds { get; init; }
}