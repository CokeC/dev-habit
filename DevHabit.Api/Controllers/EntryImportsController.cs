using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.EntryImports;
using DevHabit.Api.DTOs.Entrys;
using DevHabit.Api.Entities;
using DevHabit.Api.Jobs;
using DevHabit.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Quartz;

namespace DevHabit.Api.Controllers;

[Route("entries/imports")]
[ApiController]
public sealed class EntryImportsController(
    ApplicationDbContext dbContext,
    ISchedulerFactory schedulerFactory,
    LinkService linkService,
    UserContext userContext
    ) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EntryImportJobDto>> CreateImportJob(
        [FromForm] CreateEntryImportJobDto createImportJobDto,
        [FromHeader] AcceptHeaderDto acceptHeader,
        IValidator<CreateEntryImportJobDto> validator)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await validator.ValidateAsync(createImportJobDto);

        using var memoryStream = new MemoryStream();
        await createImportJobDto.File.CopyToAsync(memoryStream);

        var importJob = new EntryImportJob
        {
            Id = $"ei_{Guid.CreateVersion7()}",
            UserId = userId,
            Status = EntryImportStatus.Pending,
            FileName = createImportJobDto.File.FileName,
            FileContent = memoryStream.ToArray(),
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.EntryImportJobs.Add(importJob);
        await dbContext.SaveChangesAsync();

        var scheduler = await schedulerFactory.GetScheduler();
        var jobDetail = JobBuilder.Create<ProcessEntryImportJob>()
            .WithIdentity($"process-entry-import-{importJob.Id}")
            .UsingJobData("importJobId", importJob.Id)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"process-entry-import-trigger-{importJob.Id}")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(jobDetail, trigger);

        var importJobDto = importJob.ToDto();

        if (acceptHeader.IncludeLinks)
            importJobDto.Links = CreateLinksForImportJob(importJob.Id);

        return CreatedAtAction(nameof(GetImportJob), new { id = importJobDto.Id }, importJobDto);
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResult<EntryImportJobDto>>> GetImportJobs(
        [FromHeader] AcceptHeaderDto acceptHeader,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var query = dbContext.EntryImportJobs
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAtUtc);

        var totalCount = await query.CountAsync();

        var importJobDtos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(EntryImportQueries.ProjectToDto())
            .ToListAsync();

        if (acceptHeader.IncludeLinks)
        {
            foreach(var dto in importJobDtos)
            {
                dto.Links = CreateLinksForImportJob(dto.Id);
            }
        }

        var result = new PaginationResult<EntryImportJobDto>
        {
            Items = importJobDtos.AsReadOnly(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        if (acceptHeader.IncludeLinks)
        {
            result.Links = CreateLinksForImportJobs(page, pageSize, result.HasNextPage, result.HasPreviousPage).ToArray();
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntryImportJobDto>> GetImportJob(
        string id,
        [FromHeader] AcceptHeaderDto acceptHeader)
    {
        var userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var importJob = await dbContext.EntryImportJobs
            .Where(e => e.Id == id && e.UserId == userId)
            .Select(EntryImportQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (importJob == null)
            return NotFound();

        if (acceptHeader.IncludeLinks)
        {
            importJob.Links = CreateLinksForImportJob(id);
        }
        return Ok(importJob);
    }

    private List<LinkDto> CreateLinksForImportJobs(int page, int pageSize, bool hasNextPage, bool hasPreviousPage)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetImportJobs), "self", HttpMethods.Get, new{
                page = page,
                pageSize = pageSize
            })
            ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetImportJobs), "next-page", HttpMethods.Get, new
            {
                page = page + 1,
                pageSize = pageSize,
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetImportJobs), "previous-page", HttpMethods.Get, new
            {
                page = page - 1,
                pageSize = pageSize,
            }));
        }

        return links;
    }

    private LinkDto[] CreateLinksForImportJob(string id)
    {
        LinkDto[] links = [
            linkService.Create(nameof(GetImportJob), "self", HttpMethods.Get, new{id})];
        return links;
    }
}
