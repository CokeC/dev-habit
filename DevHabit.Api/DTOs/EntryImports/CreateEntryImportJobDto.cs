using FluentValidation;

namespace DevHabit.Api.DTOs.EntryImports;

public sealed class CreateEntryImportJobDto
{
    public required IFormFile File { get; init; }
}

public sealed class CreateEntryImportJobDtoValidator : AbstractValidator<CreateEntryImportJobDto>
{
    private const int MaxFileSizeInMegabytes = 10;
    private const int MaxFileSizeInBytes = MaxFileSizeInMegabytes * 1024 * 1024;

    public CreateEntryImportJobDtoValidator()
    {
        RuleFor(e => e.File)
            .NotNull()
            .WithMessage("文件为空！");

        RuleFor(e => e.File.Length)
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .WithMessage($"文件必须小于{MaxFileSizeInMegabytes}MB");

        RuleFor(e => e.File.FileName)
            .Must(e => e.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .WithMessage("文件名必须以csv结尾");
    }
}