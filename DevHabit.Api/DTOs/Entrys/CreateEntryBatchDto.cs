using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.DTOs.Entrys;

public class CreateEntryBatchDto
{
    public required List<CreateEntryDto> Entries { get; init; }
}

public sealed class CreateEntryBatchDtoValidator : AbstractValidator<CreateEntryBatchDto>
{
    public CreateEntryBatchDtoValidator(CreateEntryDtoValidator entryValidator)
    {
        RuleFor(e => e.Entries)
            .NotEmpty()
            .WithMessage("需要至少1个条目！")
            .Must(e => e.Count <= 20)
            .WithMessage("最多支持20个条目！");

        RuleForEach(e => e.Entries)
            .SetValidator(entryValidator);//重用单个条目的验证
    }
}