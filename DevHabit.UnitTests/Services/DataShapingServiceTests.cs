using DevHabit.Api.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevHabit.UnitTests.Services;

public class DataShapingServiceTests
{
    private readonly DataShapingService _dataShapingService = new();

    private sealed record TestDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int Value { get; init; }
    }

    [Fact]
    public void ShapeData_ShouldReturnAllProperties()
    {
        var entity = new TestDto
        {
            Id = "1",
            Name = "Name",
            Description = "Description",
            Value = 1
        };

        var result = _dataShapingService.ShapeData(entity, null);

        var dict = (IDictionary<string, object?>)result;

        Assert.Equal(4, dict.Count);
        Assert.Equal("Name", dict["Name"]);
        Assert.Equal("Description", dict["Description"]);
        Assert.Equal("1", dict["Id"]);
        Assert.Equal(1, dict["Value"]);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("id,name", true)]
    [InlineData("ID,NAME", true)]
    [InlineData("id,invalidField", false)]
    [InlineData("name,INVALIDFIELD", false)]
    public void Validate_ShouldReturnExpectedResult_WhenValidatingFields(string? fields, bool expectedResult)
    {
        var result = _dataShapingService.Validate<TestDto>(fields);
        Assert.Equal(expectedResult, result);
    }
}
