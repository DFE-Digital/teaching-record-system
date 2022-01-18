using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;
using BaseRateLimitConfiguration = AspNetCoreRateLimit.RateLimitConfiguration;

namespace DqtApi.Security
{
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

            this.ClientResolvers.Add(new ApiClientResolveContributor());
        }
    }
}
