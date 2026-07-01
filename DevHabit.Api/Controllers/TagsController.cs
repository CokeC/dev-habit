using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DevHabit.Api.Controllers;

[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
[Route("tags")]
[ApiController]
public sealed class TagsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags([FromHeader] AcceptHeaderDto acceptHeader)
    {
        var tags = await dbContext.Tags.Select(e => e.ToTagDto()).ToListAsync();

        var tagsCollectionDto = new TagsCollectionDto
        {
            Data = tags.AsReadOnly()
        };

        if (acceptHeader.IncludeLinks)
        {
            tagsCollectionDto.Links = CreateLinksForTags(tags.Count);
            foreach(var tagDto in tagsCollectionDto.Data)
            {
                tagDto.Links = CreateLinksForTag(tagDto.Id);
            }
        }

        return Ok(tagsCollectionDto);
    }

    private List<LinkDto> CreateLinksForTag(string id)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetTag), "self", HttpMethods.Get, new {id})
            ];
        return links;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id)
    {
        var tag = await dbContext.Tags.Where(e => e.Id == id).Select(e => e.ToTagDto()).FirstOrDefaultAsync();
        if (tag is null)
            return NotFound();
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto request, IValidator<CreateTagDto> validator, ProblemDetailsFactory detailsFactory)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var problem = detailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest);
            problem.Extensions.Add("errors", validationResult.ToDictionary());
            return BadRequest(problem);


            //return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
            

        var tag = request.ToTag();
        if (await dbContext.Tags.AnyAsync(e => e.Name == tag.Name))
            return Conflict($"Tag{tag.Name}已经存在！");
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        var habitDto = tag.ToTagDto();
        return CreatedAtAction(nameof(GetTag), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto request, InMemoryETagStore eTagStore)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(e => e.Id == id);
        if (tag is null)
            return NotFound();
        tag.UpdateFromDto(request);
        await dbContext.SaveChangesAsync();

        eTagStore.SetETag(Request.Path.Value!, tag.ToTagDto());

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(e => e.Id == id);
        if (tag is null)
            return NotFound();

        dbContext.Tags.Remove(tag);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }


    private List<LinkDto> CreateLinksForTags(int tagsCount)
    {
        List<LinkDto> links = [
            linkService.Create(nameof(GetTags), "self", HttpMethods.Get)
            ];
        if(tagsCount < 5)
        {
            links.Add(linkService.Create(nameof(CreateTag), "create", HttpMethods.Post));
        }
        return links;
    }
}
