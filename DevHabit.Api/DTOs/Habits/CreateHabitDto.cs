using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.DTOs.Habits;

public sealed record CreateHabitDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }

    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }

    public Habit ToHabit(string userId)
    {
        return new()
        {
            Id = $"h_{Guid.CreateVersion7()}",//uuid7可排序，对分页有帮助
            UserId = userId,
            Name = Name,
            Description = Description,
            Type = Type,
            Frequency = new()
            {
                Type = Frequency.Type,
                TimesPerPeriod = Frequency.TimesPerPeriod
            },
            Target = new()
            {
                Value = Target.Value,
                Unit = Target.Unit
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = EndDate,
            Milestone = Milestone != null ? new()
            {
                Target = Milestone.Target,
                Current = 0
            } : null,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits =
    [
        "minutes", "hours", "steps", "km", "cal", "pages", "books", "tasks", "sessions"
    ];

    private static readonly string[] AllowedUnitsForBinaryHabits = ["sessions", "tasks"];

    public CreateHabitDtoValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty().MinimumLength(3).MaximumLength(100)
            .WithMessage("Habit名称必须在3~100之间");

        RuleFor(e => e.Description)
            .MaximumLength(500)
            .When(e => e.Description is not null)
            .WithMessage("Habit描述不能超过500字符");

        RuleFor(e => e.Type)
            .IsInEnum()
            .WithMessage("不支持的类型");

        RuleFor(e => e.Frequency.Type)
            .IsInEnum()
            .WithMessage("不支持的类型");

        RuleFor(e => e.Frequency.TimesPerPeriod)
            .GreaterThan(0)
            .WithMessage("频率应大于0");

        RuleFor(e => e.Target.Value)
            .GreaterThan(0)
            .WithMessage("目标应大于0");

        RuleFor(e => e.Target.Unit)
            .NotEmpty()
            .Must(e => AllowedUnits.Contains(e.ToLowerInvariant()))
            .WithMessage($"单位应包含在：{string.Join(", ", AllowedUnits)}");

        RuleFor(e => e.EndDate)
            .Must(e => e is null || e.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("结束时间不能小于现在");

        When(e => e.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
            .GreaterThan(0)
            .WithMessage("里程碑应大于0");
        });

        RuleFor(e => e.Target.Unit)
            .Must((dto, unit) => IsTargetUnitCompatibleWithType(dto.Type, unit))
            .WithMessage("目标应与类型匹配");
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        var normalizedUnit = unit.ToLowerInvariant();
        return type switch
        {
            HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizedUnit),
            HabitType.Measurable => AllowedUnits.Contains(normalizedUnit),
            _ => false
        };
    }
}