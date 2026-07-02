using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.Api.Services;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentRequestAttribute : Attribute, IAsyncActionFilter
{
    private const string IdempotencyKeyHeader = "Idempotent-Key";
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(10);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if(!context.HttpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKeyValue) || !Guid.TryParse(idempotencyKeyValue, out var idempotencyKey))
        {
            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext,
                StatusCodes.Status400BadRequest,
                "请求错误！",
                detail: $"无{IdempotencyKeyHeader}头。");
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }
        //实际生产中建议使用分布式缓存，而不是下述内存缓存
        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        var cacheKey = $"idempotence:{idempotencyKey}";
        var statusCode = cache.Get<int?>(cacheKey);
        if(statusCode != null)
        {
            var result = new StatusCodeResult(statusCode.Value);
            context.Result = result;
            return;
        }

        var executedContext = await next();

        if(executedContext.Result is ObjectResult objectResult)
        {
            cache.Set(cacheKey, objectResult.StatusCode, DefaultCacheDuration);
        }
    }
}