using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.WebUtilities;

namespace TeachingRecordSystem.ServiceDefaults.Infrastructure.Logging;

public class UrlRedactor(IHttpContextAccessor httpContextAccessor)
{
    private const string RedactedContent = "***";

    private static readonly HashSet<string> _knownPiiFieldNames = new(
        [
            "trn",
            "birthdate",
            "dateofbirth",
            "firstname",
            "middlename",
            "lastname",
            "previousfirstname",
            "previousmiddlename",
            "previouslastname",
            "slugid",
            "email",
            "emailaddress",
            "nationalinsurancenumber",
            "nino",
            "teacherid",
        ],
        StringComparer.OrdinalIgnoreCase);

    public string GetScrubbedRequestUrl()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
        var endpoint = httpContext.GetEndpoint();

        var path = httpContext.Request.Path;
        var queryString = httpContext.Request.QueryString.ToString();

        var request = httpContext.Request;

        return UriHelper.BuildAbsolute(
            request.Scheme,
            request.Host,
            request.PathBase,
            RedactPiiFromUrlPath(path, endpoint, httpContext),
            new QueryString(RedactPiiFromUrlQueryString(queryString, endpoint)));
    }

    public string GetScrubbedRequestPath()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
        var endpoint = httpContext.GetEndpoint();

        var path = httpContext.Request.Path;
        return RedactPiiFromUrlPath(path, endpoint, httpContext);
    }

    public string GetScrubbedRequestPathAndQuery()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
        var endpoint = httpContext.GetEndpoint();

        var path = httpContext.Request.Path;
        var queryString = httpContext.Request.QueryString.ToString();
        return $"{RedactPiiFromUrlPath(path, endpoint, httpContext)}{RedactPiiFromUrlQueryString(queryString, endpoint)}";
    }

    public string GetScrubbedQueryString()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
        var endpoint = httpContext.GetEndpoint();

        var queryString = httpContext.Request.QueryString.ToString();
        return RedactPiiFromUrlQueryString(queryString, endpoint);
    }

    private static RedactedParametersMetadata GetRedactedParametersMetadata(Endpoint? endpoint) =>
        endpoint?.Metadata.GetMetadata<RedactedParametersMetadata>() ?? new();

    private static string RedactPiiFromUrlPath(string path, Endpoint? endpoint, HttpContext httpContext)
    {
        if (endpoint is null || endpoint is not RouteEndpoint routeEndpoint)
        {
            return path;
        }

        var endpointMetadata = GetRedactedParametersMetadata(endpoint);

        foreach (var parameter in routeEndpoint.RoutePattern.Parameters)
        {
            if (parameter is RoutePatternParameterPart routePatternParameterPart &&
                (_knownPiiFieldNames.Contains(routePatternParameterPart.Name) || endpointMetadata.ParameterNames.Contains(routePatternParameterPart.Name)))
            {
                var routeValue = httpContext.GetRouteValue(routePatternParameterPart.Name)!.ToString()!;
                path = path.Replace(routeValue, RedactedContent);
            }
        }

        return path;
    }

    private static string RedactPiiFromUrlQueryString(string queryString, Endpoint? endpoint)
    {
        var endpointMetadata = GetRedactedParametersMetadata(endpoint);

        var query = QueryHelpers.ParseQuery(queryString);

        foreach (var key in query.Keys.ToArray())
        {
            if (_knownPiiFieldNames.Contains(key) || endpointMetadata.ParameterNames.Contains(key) == true)
            {
                query[key] = RedactedContent;
            }
        }

        var scrubbedQueryString = new QueryBuilder(query).ToString();
        return scrubbedQueryString;
    }
}
