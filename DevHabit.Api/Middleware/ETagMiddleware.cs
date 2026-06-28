using DevHabit.Api.Services;
using Microsoft.EntityFrameworkCore.Design.Internal;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace DevHabit.Api.Middleware;

public sealed class ETagMiddleware(RequestDelegate next)
{
    private static readonly string[] ConcurrencyCheckMethods = [
        HttpMethods.Put,
        HttpMethods.Patch
        ];//支持乐观锁的Http方法数组
    
    public async Task InvokeAsync(HttpContext context, InMemoryETagStore eTagStore)
    {
        var isGetMethod = CheckGetMethod(context);
        var isSpecialMethods = CheckSpecialMethods();

        if (isSpecialMethods)
        {
            var resourceUri = context.Request.Path.Value!;
            var ifMatch = context.Request.Headers.IfMatch.FirstOrDefault()?.Replace("\"", "");
            if (!string.IsNullOrEmpty(ifMatch))
            {
                var currentETag = eTagStore.GetETag(resourceUri);
                if (!string.IsNullOrWhiteSpace(currentETag) && ifMatch != currentETag)
                {
                    context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                    context.Response.ContentLength = 0;
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentLength = 0;
                return;
            }
        }
        else if (!isGetMethod)
        {
            await next(context);
            return;
        }
        

        var initialResponseBody = context.Response.Body;
        using var memoryStreamOfResponseBody = new MemoryStream();
        context.Response.Body = memoryStreamOfResponseBody;

        await next(context);

        context.Response.Body = initialResponseBody;//MemoryStream在本方法内结束就会销毁，所以要将Response.Body尽快指向原来的Stream

        var isJsonContent = CheckResponseContentType(context);

        if (!isJsonContent)
        {
            await CopyStream(memoryStreamOfResponseBody, initialResponseBody);
            return;
        }

        var etag = await GenerateETag(memoryStreamOfResponseBody);

        StoreETag();

        SetResponseHeaders();

        var ifNoneMatch = GetIfNoneMatch();
        if (ifNoneMatch == etag)
            SetResponseOthers(); 
        else
            await CopyStream(memoryStreamOfResponseBody, initialResponseBody);

        #region Internal Methods

        bool CheckSpecialMethods()
        {
            return ConcurrencyCheckMethods.Contains(context.Request.Method);
        }

        void StoreETag()
        {
            var resourceUri = context.Request.Path.Value!;
            eTagStore.SetETag(resourceUri, etag);
        }

        void SetResponseHeaders()
        {
            context.Response.Headers.ETag = $"\"{etag}\"";
        }

        string? GetIfNoneMatch()
        {
            return context.Request.Headers.IfNoneMatch.FirstOrDefault()?.Replace("\"", "");
        }

        void SetResponseOthers()
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            context.Response.ContentLength = 0;
        }
        #endregion
    }

    private static bool CheckGetMethod(HttpContext context)
    {
        var isGetMethod = context.Request.Method == HttpMethods.Get;
        return isGetMethod;
    }

    private static bool CheckResponseContentType(HttpContext context)
    {
        var isStatusOK = context.Response.StatusCode == StatusCodes.Status200OK;
        if (!isStatusOK)
            return false;

        var contentType = context.Response.Headers.ContentType.FirstOrDefault();
        if (contentType == null)
            return false;

        var isJsonContent = contentType.Contains("json", StringComparison.OrdinalIgnoreCase);

        return isJsonContent;
    }

    private static async Task<string> GenerateETag(MemoryStream memoryStream)
    {
        var content = await ReadResponseBody(memoryStream);
        var hash = SHA512.HashData(content);
        return Convert.ToHexString(hash);
    }

    private static async Task<byte[]> ReadResponseBody(MemoryStream memoryStream)
    {
        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        return Encoding.UTF8.GetBytes(content);
    }

    private static async Task CopyStream(MemoryStream originStream, Stream destinationStream)
    {
        originStream.Position = 0;
        await originStream.CopyToAsync(destinationStream);
    }
}