using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.TestCommon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestTrnGeneration(this IServiceCollection services)
    {
        services.AddSingleton<TestTrnGenerator>();

        return services;
    }
}
