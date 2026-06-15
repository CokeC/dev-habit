using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

public static class TagMappings
{
    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdateAtUtc = DateTime.UtcNow;
    }
}
