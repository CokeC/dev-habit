
using DevHabit.Api.Collections;
using DevHabit.Api.DTOs.EntryImports;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DevHabit.IntegrationTests.Tests;

public class EntryImportsTest(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task CreateImportJob_ShouldSucceed()
    {
        var client = await CreateAuthenticatedClientAsync();
        var createDto = new CreateHabitDto
        {
            Name = "name",
            Description = "description",
            Type = HabitType.Measurable,
            Frequency = new FrequencyDto
            {
                Type = FrequencyType.Daily,
                TimesPerPeriod = 1
            },
            Target = new TargetDto
            {
                Value = 30,
                Unit = "pages"
            }
        };
        var createResponse = await client.PostAsJsonAsync(Routes.Habits.Create, createDto, TestContext.Current.CancellationToken);
        var habit = await createResponse.Content.ReadFromJsonAsync<HabitDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(habit);

        string csvContent = $"""
            habit_id,date,notes
            {habit.Id},2024-01-01,Started
            {habit.Id},2024-01-02,Started
            {habit.Id},2024-01-03,Getting
            """;

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new("text/csv");
        content.Add(fileContent, "file", "entries.csv");

        var response = await client.PostAsync("entries/imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var result = await response.Content.ReadFromJsonAsync<EntryImportJobDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(EntryImportStatus.Pending, result.Status);
    }
    /*
    public static TheoryData<string> ProtectedEndpoints => [
        Routes.Habits.Create,
        Routes.Auth.Register
        ];

    [Theory]
    [MemberData(nameof(ProtectedEndpoints))]
    public async Task Endpoints_ShouldRequireAuthentication(string route)
    {

    }*/
}
