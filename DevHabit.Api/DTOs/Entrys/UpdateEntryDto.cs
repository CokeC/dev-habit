using DevHabit.Api.Collections;
using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.DTOs.Entrys;

public class UpdateEntryDto
{
    public string? HabitId { get; set; }
    public string? UserId { get; set; }
    public required int Value { get; set; }
    public string? Notes { get; set; }
    public string? ExternalId { get; set; }
    public required bool IsArchived { get; set; }
    public required DateOnly Date { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class UpdateEntryDtoValidator : AbstractValidator<UpdateEntryDto>
{
    public UpdateEntryDtoValidator()
    {
        RuleFor(e => e.Value).NotEmpty();
    }
}