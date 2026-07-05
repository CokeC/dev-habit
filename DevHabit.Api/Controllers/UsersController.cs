using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DevHabit.Api.Controllers;

[Route("users")]
[ApiController]
[Authorize(Roles = Roles.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public sealed class UsersController(ApplicationDbContext dbContext, UserContext userContext) : ControllerBase
{
    /// <summary>
    /// 获得用户信息
    /// </summary>
    /// <param name="id">用户唯一标识符</param>
    /// <returns>用户信息</returns>
    [HttpGet("{id}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (id != userId)
            return Forbid();

        var user = await dbContext.Users
            .Where(u => u.Id == id)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();
        if (user is null)
            return NotFound();
        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();
        
        var user = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();
        if (user is null)
            return NotFound();
        return Ok(user);
    }
}