
using DevHabit.Api.DTOs.Entrys;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public class CreateEntryBatchDtoValidatorTests
{
    private readonly CreateEntryBatchDtoValidator _validator;
    private readonly CreateEntryDtoValidator _entryValidator = new();

    public CreateEntryBatchDtoValidatorTests()
    {
        _validator = new CreateEntryBatchDtoValidator(_entryValidator);
    }

    [Fact]
    public async Task Validate_ShouldNotReturnError()
    {
        var dto = new CreateEntryBatchDto
        {
            Entries = [
                new(){
                    HabitId = $"h_{Guid.CreateVersion7()}",
                    Value = 1,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow)
                }
                ]
        };

        var result = await _validator.TestValidateAsync(dto, null, CancellationToken.None);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_EntriesEmpty()
    {
        var dto = new CreateEntryBatchDto
        {
            Entries = []
        };

        var result = await _validator.TestValidateAsync(dto, null, CancellationToken.None);

        result.ShouldHaveValidationErrorFor(e => e.Entries);
    }
}
