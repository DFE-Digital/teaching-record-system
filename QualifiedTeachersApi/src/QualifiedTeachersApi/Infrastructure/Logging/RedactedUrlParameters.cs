using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace QualifiedTeachersApi.Infrastructure.Logging;

internal sealed class RedactedUrlParameters
{
    public RedactedUrlParameters()
    {
        QueryParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public ISet<string> QueryParameters { get; }

    public string ScrubUrl(string url)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        if (!url.Contains("?"))
        {
            return url;
        }

        var path = url.Split('?')[0];
        var queryString = url.Split('?')[1];

        var scrubbedQueryString = ScrubQueryString(queryString);
        return $"{path}{scrubbedQueryString}";
    }

    public string ScrubQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
        {
            return string.Empty;
        }

        var query = QueryHelpers.ParseQuery(queryString);

        foreach (var key in query.Keys.ToArray())
        {
            if (QueryParameters.Contains(key))
            {
                query[key] = "**REDACTED**";
            }
        }

        var scrubbedQueryString = new QueryBuilder(query).ToString();
        return scrubbedQueryString;
    }
}
