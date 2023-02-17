using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using QualifiedTeachersApi.Logging;

namespace QualifiedTeachersApi;

public static class HttpRequestExtensions
{
    public static string GetScrubbedRequestUrl(this HttpRequest httpRequest)
    {
        if (httpRequest is null)
        {
            throw new ArgumentNullException(nameof(httpRequest));
        }

        var httpContext = httpRequest.HttpContext;
        var requestUrl = httpRequest.GetEncodedUrl();

        var redactedUrlParameters = httpContext.GetEndpoint()?.Metadata?.GetMetadata<RedactedUrlParameters>();

        return redactedUrlParameters?.ScrubUrl(requestUrl) ?? requestUrl;
    }

    public static string GetScrubbedRequestPathAndQuery(this HttpRequest httpRequest)
    {
        if (httpRequest is null)
        {
            throw new ArgumentNullException(nameof(httpRequest));
        }

        var httpContext = httpRequest.HttpContext;
        var pathAndQuery = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";

        var redactedUrlParameters = httpContext.GetEndpoint()?.Metadata?.GetMetadata<RedactedUrlParameters>();

        return redactedUrlParameters?.ScrubUrl(pathAndQuery) ?? pathAndQuery;
    }

    public static string GetScrubbedQueryString(this HttpRequest httpRequest)
    {
        if (httpRequest is null)
        {
            throw new ArgumentNullException(nameof(httpRequest));
        }

        var httpContext = httpRequest.HttpContext;
        var queryString = httpContext.Request.QueryString.ToString();

        var redactedUrlParameters = httpContext.GetEndpoint()?.Metadata?.GetMetadata<RedactedUrlParameters>();

        return redactedUrlParameters?.ScrubQueryString(queryString) ?? queryString;
    }
}
