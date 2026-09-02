using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Inductions;

public static class Extensions
{
    public static IServiceCollection AddInductionService(this IServiceCollection services)
    {
        services.AddTransient<InductionService>();

        return services;
    }
}
