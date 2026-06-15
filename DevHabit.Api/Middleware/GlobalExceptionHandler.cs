using Microsoft.AspNetCore.Diagnostics;

namespace DevHabit.Api.Middleware;

/// <summary>
/// 用于处理未处理的异常信息
/// </summary>
/// <param name="detailsService"></param>
public sealed class GlobalExceptionHandler(IProblemDetailsService detailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return detailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new()
            {
                Title = "Internal Server Error",
                Detail = "发生一些错误，请稍后重试！"
            }
        });
    }
}
