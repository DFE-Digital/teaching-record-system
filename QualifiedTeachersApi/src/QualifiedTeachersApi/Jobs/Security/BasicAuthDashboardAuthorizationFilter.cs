using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;

namespace QualifiedTeachersApi.Jobs.Security;

public class BasicAuthDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public const string AuthenticationHeaderScheme = "Basic";

    private readonly BasicAuthDashboardAuthorizationFilterOptions _options;

    public BasicAuthDashboardAuthorizationFilter(BasicAuthDashboardAuthorizationFilterOptions options)
    {
        _options = options;
    }

    public bool Authorize(DashboardContext _context)
    {
        var context = _context.GetHttpContext();
        string? header = context.Request.Headers["Authorization"];

        if (!string.IsNullOrWhiteSpace(header))
        {
            var authValues = AuthenticationHeaderValue.Parse(header);

            if (authValues.Scheme.Equals(AuthenticationHeaderScheme, StringComparison.OrdinalIgnoreCase))
            {
                var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter!));
                var parts = parameter.Split(':');

                if (parts.Length > 1)
                {
                    string username = parts[0];
                    string password = parts[1];

                    if ((!string.IsNullOrWhiteSpace(username)) && (!string.IsNullOrWhiteSpace(password))
                        && username.Equals(_options.Username, _options.LoginCaseSensitive ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase)
                        && password.Equals(_options.Password, _options.LoginCaseSensitive ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        context.Response.StatusCode = 401;
        context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        return false;
    }
}
