using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Entities;
using Newtonsoft.Json;

namespace DevHabit.Api.DTOs.Habits;

public record HabitDto : ILinksResponse
{
    //用“required”标记为必需，避免数据库映射时遗漏属性
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }

    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public required HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAtUtc { get; init; }

    public LinkDto[] Links { get; set; } = [];
}

public sealed record HabitWithTagsDto : HabitDto
{
    [JsonProperty(Order = int.MaxValue)]
    public required string[] Tags { get; init; }
}

public sealed record FrequencyDto
{
    public required FrequencyType Type { get; init; }
    public required int TimesPerPeriod { get; init; }
}

public sealed class TargetDto
{
    public required int Value { get; init; }
    public required string Unit { get; init; }
}

public sealed class MilestoneDto
{
    public required int Target { get; init; }
    public required int Current { get; init; }
}