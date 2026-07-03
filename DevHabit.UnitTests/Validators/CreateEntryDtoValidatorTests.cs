using DevHabit.Api.DTOs.Entrys;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevHabit.UnitTests.Validators;

public class CreateEntryDtoValidatorTests
{
    public readonly CreateEntryDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldSucceed()
    {
        //Arrange
        var dto = new CreateEntryDto
        {
            HabitId = $"h_{Guid.CreateVersion7()}",
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        //Act
        var validationResult = await _validator.ValidateAsync(dto, CancellationToken.None);

        //Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task Validate_ShouldFail_HabitIdEmpty()
    {
        //Arrange
        var dto = new CreateEntryDto
        {
            HabitId = string.Empty,
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        //Act
        var validationResult = await _validator.ValidateAsync(dto, CancellationToken.None);

        //Assert
        Assert.False(validationResult.IsValid);
        var validationFailure = Assert.Single(validationResult.Errors);
        Assert.Equal(nameof(CreateEntryDto.HabitId), validationFailure.PropertyName);
    }
}