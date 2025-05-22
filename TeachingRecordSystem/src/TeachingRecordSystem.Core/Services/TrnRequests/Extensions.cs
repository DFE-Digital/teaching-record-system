using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public static class Extensions
{
    public static IServiceCollection AddTrnRequestService(this IServiceCollection services)
    {
        services.AddTransient<TrnRequestService>();
        return services;
    }

    public static IHostApplicationBuilder AddTrnRequestService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTrnRequestService();
        return builder;
    }
}
