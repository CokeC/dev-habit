using Asp.Versioning;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Mime;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.Api.Controllers;

[Route("habits")]
[ApiController]
[ApiVersion(1.0)]
[Produces(MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.JsonV2,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1,
    CustomMediaTypeNames.Application.HateoasJsonV2)]
public sealed class HabitsController(ApplicationDbContext dbContext, LinkService linkService, UserContext userContext) : ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> GetHabits(HabitsQueryParameters query, SortMappingProvider sortMappingProvider, DataShapingService shapingService)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();
        
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: $"排序参数不合规：{query.Sort}");

        if(!shapingService.Validate<HabitDto>(query.Fields))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: $"塑形参数不合规：{query.Fields}");

        var sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        var habitsQuery = dbContext.Habits.Where(e => e.UserId == userId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            query.Search = query.Search.Trim().ToLower();
            habitsQuery = habitsQuery.Where(e => e.Name.ToLower().Contains(query.Search)
                || e.Description != null && e.Description.ToLower().Contains(query.Search));
        }
        if (query.Type != null)
            habitsQuery = habitsQuery.Where(e => e.Type == query.Type);
        if(query.Status != null)
            habitsQuery = habitsQuery.Where(e => e.Status == query.Status);

        habitsQuery = habitsQuery.ApplySort(query.Sort, sortMappings);
        

        var habitDtos = habitsQuery.Select(e => e.ToHabitDto());

        int totalCount = await habitsQuery.CountAsync();
        var habits = await habitDtos
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize).ToListAsync();
                
        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = shapingService.ShapeCollectionData(habits, query.Fields, query.IncludeLinks ? h => CreateLinksForHabit(h.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
        if(query.IncludeLinks)
            paginationResult.Links = CreateLinksForHabits(query, paginationResult.HasNextPage, paginationResult.HasPreviousPage);

        //var paginationResult = await PaginationResult<HabitDto>.CreateAsync(habitDtos, query.Page, query.PageSize);
        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    //[MapToApiVersion(1.0)]
    public async Task<IActionResult> GetHabit(string id, [FromQuery] HabitsQueryParameters query, DataShapingService shapingService)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!shapingService.Validate<HabitDto>(query.Fields))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: $"塑形参数不合规：{query.Fields}");

        var habit = await dbContext.Habits.Where(e => e.Id == id && e.UserId == userId).Select(e => e.ToHabitWithTagsDto()).FirstOrDefaultAsync();
        
        if (habit is null)
            return NotFound();

        var shapedHabitDto = shapingService.ShapeData(habit, query.Fields);

        if(query.IncludeLinks)
        {
            var links = CreateLinksForHabit(id, query.Fields);

            shapedHabitDto.TryAdd("links", links);
        }

        return Ok(shapedHabitDto);
    }

    [HttpGet("{id}")]
    [ApiVersion(2.0)]
    public async Task<IActionResult> GetHabitV2(string id, string? fields, [FromHeader(Name = "Accept")] string? accept, DataShapingService shapingService)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!shapingService.Validate<HabitDto>(fields))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: $"塑形参数不合规：{fields}");

        var habit = await dbContext.Habits.Where(e => e.Id == id && e.UserId == userId).Select(e => e.ToHabitWithTagsDtoV2()).FirstOrDefaultAsync();

        if (habit is null)
            return NotFound();

        var shapedHabitDto = shapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJson)
        {
            var links = CreateLinksForHabit(id, fields);

            shapedHabitDto.TryAdd("links", links);
        }

        return Ok(shapedHabitDto);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto request, IValidator<CreateHabitDto> validator, ProblemDetailsFactory detailsFactory)
    {
        /*var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var problem = detailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest);
            problem.Extensions.Add("errors", validationResult.ToDictionary());
            return BadRequest(problem);
        }*/
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        //下面的这个验证直接抛出异常，用于替换上面的验证处理
        await validator.ValidateAndThrowAsync(request);

        var habit = request.ToHabit(userId);
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();
        var habitDto = habit.ToHabitDto();

        habitDto.Links = CreateLinksForHabit(habit.Id, null);

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto request)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var habit = await dbContext.Habits.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (habit is null)
            return NotFound();
        habit.UpdateFromDto(request);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }    

    //Patch方法不常用，常用Put代替
    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patch)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var habit = await dbContext.Habits.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (habit is null)
            return NotFound();

        var habitDto = habit.ToHabitDto();

        patch.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
            return ValidationProblem(ModelState);

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var habit = await dbContext.Habits.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (habit is null)
            return NotFound();

        dbContext.Habits.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private LinkDto[] CreateLinksForHabit(string id, string? fields)
    {
        LinkDto[] links = [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new{id, fields}),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new{id}),
            linkService.Create(nameof(PatchHabit), "partial-update", HttpMethods.Patch, new{id}),
            linkService.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new{id}),
            linkService.Create(nameof(HabitTagsController.UpsertHabitTags), "upsert-tags", HttpMethods.Put, new{ habitID = id}, HabitTagsController.Name),
            ];
        return links;
    }

    private LinkDto[] CreateLinksForHabits(HabitsQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        LinkDto[] links = [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new{
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
            ];

        if (hasNextPage)
        {
            links = links.Append(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            })).ToArray();
        }

        if (hasPreviousPage)
        {
            links = links.Append(linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            })).ToArray();
        }

        return links;
    }
}
