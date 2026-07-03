
using DevHabit.Api.DTOs.EntryImports;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using NSubstitute;


namespace DevHabit.UnitTests.Services;

public class CreateEntryImportJobDtoValidatorTests
{
    private readonly CreateEntryImportJobDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError()
    {
        var dto = new CreateEntryImportJobDto
        {
            File = CreateFormFile("test.csv", "text/csv", 1024)
        };

        var result = await _validator.TestValidateAsync(dto, null, CancellationToken.None);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static IFormFile CreateFormFile(string filename, string contentType, long length)
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(filename);
        formFile.ContentType.Returns(contentType);
        formFile.Length.Returns(length);
        return formFile;
    }
}
