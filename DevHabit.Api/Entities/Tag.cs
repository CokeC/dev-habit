using System.Diagnostics.CodeAnalysis;

namespace DevHabit.Api.Entities;

public sealed class Tag
{
    public string Id { get; set; } = string.Empty;
    [NotNull]
    public string? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdateAtUtc { get; set; }
}