using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Something;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSomething(this IServiceCollection services)
    {
        services.AddTransient<SomethingService, SomethingService>();

        return services;
    }
}
