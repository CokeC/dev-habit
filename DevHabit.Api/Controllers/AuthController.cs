using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevHabit.Api.Controllers;

[Route("auth")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController(UserManager<IdentityUser> userManager, ApplicationDbContext appDbContext, AppIdentityDbContext IdDbContext, TokenProvider tokenProvider) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto request)
    {
        //创建事务，保证两个数据库同时成功、同时失败
        using IDbContextTransaction transaction = await IdDbContext.Database.BeginTransactionAsync();
        //连接2个数据库
        appDbContext.Database.SetDbConnection(IdDbContext.Database.GetDbConnection());
        //让2个数据库使用同一个事务实例
        await appDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        
        var identityUser = new IdentityUser
        {
            Email = request.Email,
            UserName = request.Name
        };

        var identityResult = await userManager.CreateAsync(identityUser, request.Password);

        if (!identityResult.Succeeded)
        {
            var extension = new Dictionary<string, object?>
            {
                {
                    "errors",
                    identityResult.Errors.ToDictionary(e => e.Code, e => e.Description)
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

        await transaction.CommitAsync();//如果不调用，所有更改均回滚

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);
        var accessTokens = tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto request)
    {
        var identityUser = await userManager.FindByEmailAsync(request.Email);
        if (identityUser == null || !await userManager.CheckPasswordAsync(identityUser, request.Password))
            return Unauthorized();
        var tokenRequest = new TokenRequest(identityUser.Id, request.Email);
        var accessTokens = tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }
}