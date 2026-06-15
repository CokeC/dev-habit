using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace DevHabit.Api.Middleware;

public class ValidationExceptionHandler(IProblemDetailsService problemService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var context = new ProblemDetailsContext()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new()
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "发生一些错误，请稍后重试！"
            }
        };

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
            g => g.Key.ToLowerInvariant(),
            g => g.Select(e => e.ErrorMessage)
            );

        context.ProblemDetails.Extensions.Add("errors", errors);

        return await problemService.TryWriteAsync(context);
    }
}
