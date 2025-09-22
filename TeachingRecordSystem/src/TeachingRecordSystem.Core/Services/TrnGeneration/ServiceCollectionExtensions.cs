using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrnGeneration(this IServiceCollection services)
    {
        services.AddScoped<ITrnGenerator, DbTrnGenerator>();

        return services;
    }
}
