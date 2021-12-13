using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DqtApi.Security
{
    public class ClaimsPrincipalCurrentClientProvider : ICurrentClientProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipalCurrentClientProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentClientId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var principal = httpContext?.User;
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
