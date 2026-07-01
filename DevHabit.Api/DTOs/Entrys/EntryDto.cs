using DevHabit.Api.Collections;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Entities;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevHabit.Api.DTOs.Entrys;

public class EntryDto
{
    public required string Id { get; set; }
    public required string HabitId { get; set; }
    public required string UserId { get; set; }
    public int Value { get; set; }
    public string? Notes { get; set; }
    public EntrySource Source { get; set; }
    public string? ExternalId { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public LinkDto[] Links { get; set; } = [];

    public Entry ToEntity(string userId, Habit habit)
    {
        return new()
        {
            Id = Id,
            UserId = userId,
            HabitId = habit.Id,
            Value = Value,
            Notes = Notes,
            Source = Source,
            ExternalId = ExternalId,
            IsArchived = IsArchived,
            Date = Date,
            CreatedAtUtc = DateTime.UtcNow,
            Habit = habit
        };
    }
}

public static class EntryExtensions
{
    public static EntryDto ToDto(this Entry entry)
    {
        return new()
        {
            Id = entry.Id,
            UserId = entry.UserId,
            HabitId = entry.HabitId,
            Value = entry.Value,
            Notes = entry.Notes,
            Source = entry.Source,
            ExternalId = entry.ExternalId,
            IsArchived = entry.IsArchived,
            Date = entry.Date,
            CreatedAtUtc = entry.CreatedAtUtc,
            UpdatedAtUtc = entry.UpdatedAtUtc
        };
    }

    public static void UpdateFromDto(this Entry entry, UpdateEntryDto entryDto)
    {

        entry.UserId = entryDto.UserId!;
        entry.HabitId = entryDto.HabitId!;
        entry.Value = entryDto.Value;
        entry.Notes = entryDto.Notes;
        entry.IsArchived = entryDto.IsArchived;
        entry.Date = entryDto.Date;
    }
}