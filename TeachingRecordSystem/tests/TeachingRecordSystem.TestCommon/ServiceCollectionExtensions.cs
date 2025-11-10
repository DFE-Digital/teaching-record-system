using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.TestCommon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestTrnGeneration(this IServiceCollection services)
    {
        services.AddSingleton<ITrnGenerator, TestTrnGenerator>();

        return services;
    }
}
