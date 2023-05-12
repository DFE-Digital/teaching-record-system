using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;
using BaseRateLimitConfiguration = AspNetCoreRateLimit.RateLimitConfiguration;

namespace QualifiedTeachersApi.Infrastructure.Security;

public class RateLimitConfiguration : BaseRateLimitConfiguration
{
    public RateLimitConfiguration(
        IOptions<IpRateLimitOptions> ipOptions,
        IOptions<ClientRateLimitOptions> clientOptions)
        : base(ipOptions, clientOptions)
    {
    }

    public override void RegisterResolvers()
    {
        base.RegisterResolvers();

        ClientResolvers.Add(new ApiClientResolveContributor());
    }
}
