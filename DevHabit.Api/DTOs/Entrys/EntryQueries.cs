using DevHabit.Api.Entities;
using System.Linq.Expressions;

namespace DevHabit.Api.DTOs.Entrys;

public static class EntryQueries
{
    public static Expression<Func<Entry, EntryDto>> ProjectToDto()
    {
        return u => new()
        {
            Id = u.Id,
            HabitId = u.HabitId,
            UserId = u.UserId,
            CreatedAtUtc = u.CreatedAtUtc,
            UpdatedAtUtc = u.UpdatedAtUtc
        };
    }
}