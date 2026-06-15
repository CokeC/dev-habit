using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Services;

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor contextAccessor)
{
    public LinkDto Create(string endpointName, string rel, string method, object? values = null, string? controller = null)
    {
        var href = linkGenerator.GetUriByAction(contextAccessor.HttpContext!, endpointName, controller, values);

        return new()
        {
            Href = href ?? throw new Exception("endpointName无效！"),
            Rel = rel,
            Method = method
        };
    }
}
