using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public static class Extensions
{
    public static IServiceCollection AddTrnRequestService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<TrnRequestService>();
        services.AddOptions<TrnRequestOptions>().Configure(options =>
            options.AllowContactPiiUpdatesFromUserIds = configuration.GetSection("AllowContactPiiUpdatesFromUserIds").Get<Guid[]>() ?? []);
        return services;
    }

    public static IHostApplicationBuilder AddTrnRequestService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTrnRequestService(builder.Configuration);
        return builder;
    }
}
