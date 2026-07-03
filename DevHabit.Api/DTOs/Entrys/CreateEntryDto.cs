using DevHabit.Api.Collections;
using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.DTOs.Entrys;

public class CreateEntryDto
{
    public string? HabitId { get; set; }
    public string? UserId { get; set; }
    public required int Value { get; set; }
    public string? Notes { get; set; }
    public required DateOnly Date { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Entry ToEntity(string userId, Habit habit)
    {
        return new()
        {
            Id = $"e_{Guid.CreateVersion7()}",
            UserId = userId,
            HabitId = habit.Id,
            Value = Value,
            Notes = Notes,
            IsArchived = false,
            Date = Date,
            CreatedAtUtc = DateTime.UtcNow,
            Habit = habit
        };
    }
}

public sealed class CreateEntryDtoValidator : AbstractValidator<CreateEntryDto>
{
    public CreateEntryDtoValidator()
    {
        RuleFor(e => e.HabitId).NotEmpty();
        RuleFor(e => e.Value).NotEmpty();
    }
}