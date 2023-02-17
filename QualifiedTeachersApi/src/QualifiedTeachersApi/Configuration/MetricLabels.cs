using Microsoft.Extensions.Configuration;
using Prometheus;

namespace QualifiedTeachersApi.Configuration;

public static class MetricLabels
{
    public static void ConfigureLabels(IConfiguration configuration)
    {
        if (configuration["CF_INSTANCE_INDEX"] != null)
        {
            Metrics.DefaultRegistry.SetStaticLabels(new()
            {
                { "app", configuration["VCAP_APPLICATION:application_name"] },
                { "organisation", configuration["VCAP_APPLICATION:organization_name"] },
                { "space", configuration["VCAP_APPLICATION:space_name"] },
                { "app_instance", configuration["CF_INSTANCE_INDEX"] }
            });
        }
    }
}
