using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ClaimsPrincipalCurrentUserProvider(IHttpContextAccessor httpContextAccessor, IDbContextFactory<TrsDbContext> dbContextFactory) : ICurrentUserProvider
{
    public static bool TryGetCurrentApplicationUserFromHttpContext(HttpContext httpContext, out Guid userId)
    {
        var principal = httpContext.User;

        // If there's a TRN claim then it's either an access token from ID or from Teacher Auth (i.e. AuthorizeAccess).
        if (principal.HasClaim(c => c.Type is "trn" or "trn_request_id"))
        {
            if (principal.HasClaim(c => c.Type == "scope" && c.Value.Contains("dqt:read")))
            {
                // ID access token
                var idApplicationUserId = httpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<Guid>("GetAnIdentityApplicationUserId");
                userId = idApplicationUserId;
                return true;
            }

            if (principal.FindFirstValue("trs_user_id") is string trsUserId)
            {
                // Teacher Auth access token
                userId = Guid.Parse(trsUserId);
                return true;
            }
        }

        var userIdStr = principal.FindFirstValue("sub");

        if (userIdStr is null || !Guid.TryParse(userIdStr, out userId))
        {
            userId = default;
            return false;
        }

        return true;
    }

    public Guid GetCurrentApplicationUserId()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");

        if (!TryGetCurrentApplicationUserFromHttpContext(httpContext, out var userId))
        {
            throw new Exception("No current user.");
        }

        return userId;
    }

    public bool TryGetTrnRequestId([NotNullWhen(true)] out string? trnRequestId)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");

        if (httpContext.User.FindFirst(AuthorizeAccessClaimTypes.TrnRequestId) is { Value: var claimTrnRequestId })
        {
            trnRequestId = claimTrnRequestId;
            return true;
        }

        trnRequestId = null;
        return false;
    }

    public async Task<string?> GetTrnAsync()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new Exception("No HttpContext.");

        if (httpContext.User.FindFirst(AuthorizeAccessClaimTypes.Trn) is { Value: var claimTrn })
        {
            return claimTrn;
        }

        if (httpContext.User.FindFirst(AuthorizeAccessClaimTypes.TrnRequestId) is { Value: var requestId })
        {
            var applicationUserId = GetCurrentApplicationUserId();

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var trn = await (
                    from m in dbContext.TrnRequestMetadata
                    join person in dbContext.Persons on m.ResolvedPersonId equals person.PersonId
                    where m.ApplicationUserId == applicationUserId && m.RequestId == requestId
                    select person.Trn)
                .SingleOrDefaultAsync();

            return trn;
        }

        return null;
    }
}
