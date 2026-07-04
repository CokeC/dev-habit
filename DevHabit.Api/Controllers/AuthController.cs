using DevHabit.Api.Collections;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace DevHabit.Api.Controllers;

[Route("auth")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController(UserManager<IdentityUser> userManager, ApplicationDbContext appDbContext, AppIdentityDbContext idDbContext, TokenProvider tokenProvider, IOptions<JwtAuthOptions> options) : ControllerBase
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;
    
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto request)
    {
        //创建事务，保证两个数据库同时成功、同时失败
        using IDbContextTransaction transaction = await idDbContext.Database.BeginTransactionAsync();
        //连接2个数据库
        appDbContext.Database.SetDbConnection(idDbContext.Database.GetDbConnection());
        //让2个数据库使用同一个事务实例
        await appDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        
        var identityUser = new IdentityUser
        {
            Email = request.Email,
            UserName = request.Name
        };

        var createResult = await userManager.CreateAsync(identityUser, request.Password);

        if (!createResult.Succeeded)
        {
            var extension = new Dictionary<string, object?>
            {
                {
                    "errors",
                    createResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                }
            };
            return Problem(detail: "无法注册用户，请重试！",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extension);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(identityUser, Roles.Member);
        if (!addToRoleResult.Succeeded)
        {
            var extension = new Dictionary<string, object?>
            {
                {
                    "errors",
                    addToRoleResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                }
            };
            return Problem(detail: "无法注册用户，请重试！",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extension);
        }

        var user = request.ToEntity();
        user.IdentityId = identityUser.Id;

        appDbContext.Users.Add(user);

        await appDbContext.SaveChangesAsync();
        
        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email, [Roles.Member]);
        var accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays)
        };
        idDbContext.RefreshTokens.Add(refreshToken);

        await transaction.CommitAsync();//如果不调用，所有更改均回滚

        return Ok(accessTokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto request)
    {
        var identityUser = await userManager.FindByEmailAsync(request.Email);
        if (identityUser == null || !await userManager.CheckPasswordAsync(identityUser, request.Password))
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(identityUser);

        var tokenRequest = new TokenRequest(identityUser.Id, request.Email, roles);
        var accessTokens = tokenProvider.Create(tokenRequest);

        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays)
        };
        idDbContext.RefreshTokens.Add(refreshToken);

        await idDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokensDto>> Refresh(RefreshTokenDto request)
    {
        var refreshToken = await idDbContext.RefreshTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == request.RefreshToken);

        if (refreshToken is null)
            return Unauthorized();

        if (refreshToken.ExpiresAtUtc < DateTime.UtcNow)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(refreshToken.User);

        var tokenRequest = new TokenRequest(refreshToken.User.Id, refreshToken.User.Email!, roles);

        var accessTokens = tokenProvider.Create(tokenRequest);

        //保存生成的刷新令牌
        refreshToken.Token = accessTokens.RefreshToken;
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays);

        await idDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }
}