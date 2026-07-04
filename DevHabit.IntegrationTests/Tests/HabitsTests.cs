
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace DevHabit.IntegrationTests.Tests;

public class HabitsTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task CreateHabit_ShouldSucceed()
    {
        var dto = new CreateHabitDto
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

        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(Routes.Habits.Create, dto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotNull(await response.Content.ReadFromJsonAsync<HabitDto>(TestContext.Current.CancellationToken));
    }
}