using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public static class Extensions
{
    public static IServiceCollection AddRoutesToProfessionalStatusService(this IServiceCollection services)
    {
        services.AddTransient<RoutesToProfessionalStatusService>();

        return services;
    }
}
