using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.DTOs.Tags;

public sealed record CreateTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public Tag ToTag()
    {
        return new()
        {
            Id = $"h_{Guid.CreateVersion7()}",//uuid7可排序，对分页有帮助
            Name = Name,
            Description = Description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(e => e.Name).NotEmpty().MinimumLength(3);
        RuleFor(e => e.Description).MaximumLength(50);
    }
}