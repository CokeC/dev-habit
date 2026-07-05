using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Entrys;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.FunctionalTests.Infrastructure;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DevHabit.FunctionalTests.Test;

public class HabitEntryFlowTests(DevHabitWebAppFactory factory) : FunctionalTestFixture(factory)
{
    [Fact]
    public async Task CompleteHabitEntryFlow_ShouldSucceed()
    {
        //Arrange
        await CleanupDatabaseAsync();
        const string email = "entryflow@test.com";
        const string password = "Test123!";

        //1.注册1个用户
        var client = CreateClient();
        var registerDto = new RegisterUserDto
        {
            Name = email,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        var registerResponse = await client.PostAsJsonAsync(Routes.Auth.Register, registerDto, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        //2.登陆获取token
        var loginDto = new LoginUserDto
        {
            Email = email,
            Password = password
        };
        var loginResponse = await client.PostAsJsonAsync(Routes.Auth.Login, loginDto, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AccessTokensDto>(TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens!.AccessToken);

        //3.创建一个习惯
        var habitDto = new CreateHabitDto
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

        var createHabitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createHabitResponse.StatusCode);
        var createHabit = await createHabitResponse.Content.ReadFromJsonAsync<HabitDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(createHabit);

        //4.创建1个条目
        var firstEntryDto = new CreateEntryDto
        {
            HabitId = createHabit.Id,
            Value = 25,
            Date = new DateOnly(2025,1,1)
        };
        var firstEntryResponse = await client.PostAsJsonAsync(Routes.Entries.Create, firstEntryDto, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, firstEntryResponse.StatusCode);
        var firstEntry = await firstEntryResponse.Content.ReadFromJsonAsync<EntryDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(firstEntry);
        Assert.Equal(25, firstEntryDto.Value);

        //5.创建第2个条目
        var secondEntryDto = new CreateEntryDto
        {
            HabitId = createHabit.Id,
            Value = 25,
            Date = new DateOnly(2025, 2, 1)
        };
        var secondEntryResponse = await client.PostAsJsonAsync(Routes.Entries.Create, secondEntryDto, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, secondEntryResponse.StatusCode);
        var secondEntry = await secondEntryResponse.Content.ReadFromJsonAsync<EntryDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(secondEntry);
        Assert.Equal(25, secondEntryDto.Value);

        //6.获取所有条目
        var getEntriesResponse = await client.GetAsync($"{Routes.Entries.GetAll}?habitId={createHabit.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getEntriesResponse.StatusCode);
        var entries = await getEntriesResponse.Content.ReadFromJsonAsync<PaginationResult<EntryDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(entries);
        Assert.Equal(2, entries.Items.Count);

        //7.
        var getStatsResponse = await client.GetAsync(Routes.Entries.Stats, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getStatsResponse.StatusCode);
        var stats = await getStatsResponse.Content.ReadFromJsonAsync<EntryStatsDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(stats);
        Assert.True(stats.TotalEntries >= 2);
    }
}
