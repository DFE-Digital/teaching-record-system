using System;
using DqtApi.Logging;
using Microsoft.AspNetCore.Http;

namespace DqtApi
{
    public static class HttpRequestExtensions
    {
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
}
