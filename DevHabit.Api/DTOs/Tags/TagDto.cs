using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using System.Collections.ObjectModel;

namespace DevHabit.Api.DTOs.Tags;

public sealed record TagDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdateAtUtc { get; set; }
}

public sealed record TagsCollectionDto
{
    public required ReadOnlyCollection<TagDto> Data { get; init; }
}

public static class TagExtensions
{
    public static TagDto ToTagDto(this Tag tag)
    {
        return new()
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdateAtUtc = tag.UpdateAtUtc
        };
    } 
}