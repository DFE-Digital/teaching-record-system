using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrnGeneration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddScoped<ITrnGenerator, DbTrnGenerator>();

        return services;
    }
}
