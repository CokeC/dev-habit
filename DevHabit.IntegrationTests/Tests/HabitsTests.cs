
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.IntegrationTests.Infrastructure;
using Humanizer;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;

namespace DevHabit.IntegrationTests.Tests;

public class HabitsTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task CreateHabit_ShouldSucceed()
    {
        await CleanupDatabaseAsync();//清除数据库
        
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

    [Fact]
    public async Task PatchHabit_ShouldSucceed()
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
        var createdHabit = await createResponse.Content.ReadFromJsonAsync<HabitDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdHabit);

        var pathDoc = new JsonPatchDocument<UpdateHabitDto>();
        pathDoc.Replace(e => e.Name, "patched");

        using var stringContent = new StringContent(JsonConvert.SerializeObject(pathDoc), new MediaTypeHeaderValue(MediaTypeNames.Application.JsonPatch));
        var response = await client.PatchAsync(Routes.Habits.Patch(createdHabit.Id), stringContent, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id), TestContext.Current.CancellationToken);
        var patchedHabit = await getResponse.Content.ReadFromJsonAsync<HabitWithTagsDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(patchedHabit);
        Assert.Equal("Patched Habit Name", patchedHabit.Name);
    }
}