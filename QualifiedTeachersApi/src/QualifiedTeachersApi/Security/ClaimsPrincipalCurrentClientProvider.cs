using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace QualifiedTeachersApi.Security
{
    public class ClaimsPrincipalCurrentClientProvider : ICurrentClientProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipalCurrentClientProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string GetCurrentClientIdFromHttpContext(HttpContext httpContext)
        {
            var principal = httpContext?.User;
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string GetCurrentClientId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return GetCurrentClientIdFromHttpContext(httpContext);
        }
    }
}
