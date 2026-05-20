using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.InductionExemptions;

public static class Extensions
{
    public static IServiceCollection AddInductionExemptionService(this IServiceCollection services)
    {
        services.AddTransient<InductionExemptionService>();

        return services;
    }
}
