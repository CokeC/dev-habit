using DevHabit.Api.Database;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Route("habits/{habitId}/tags")]
[ApiController]
public class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(HabitTagsController).Replace("Controller", string.Empty);
    [HttpPut]
    public async Task<ActionResult> UpsertHabitTags(string habitId, UpsertHabitTagsDto request)
    {
        var habit = await dbContext.Habits
            .Include(e => e.HabitTags)
            .FirstOrDefaultAsync(e => e.Id == habitId);
        if (habit == null)
            return NotFound();

        var currentTagIds = habit.HabitTags.Select(e => e.TagId).ToHashSet();
        if (currentTagIds.SetEquals(request.TagIds))
            return NoContent();

        var existingTagIds = await dbContext.Tags
            .Where(e => request.TagIds.Contains(e.Id))
            .Select(e => e.Id).ToListAsync();

        if (existingTagIds.Count != request.TagIds.Count)
            return BadRequest("一个或多个标签的ID无效！");

        habit.HabitTags.RemoveAll(e => !request.TagIds.Contains(e.TagId));

        var tagIdsToAdd = request.TagIds.Except(currentTagIds);
        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedDate = DateTime.UtcNow
        }));

        await dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        var habitTag = await dbContext.HabitTags
            .SingleOrDefaultAsync(e => e.HabitId == habitId && e.TagId == tagId);
        if (habitTag == null)
            return NotFound();
        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
