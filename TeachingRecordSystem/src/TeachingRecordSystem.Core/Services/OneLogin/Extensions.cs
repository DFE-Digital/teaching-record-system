using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public static class Extensions
{
    public static IServiceCollection AddOneLoginService(this IServiceCollection services)
    {
        services.AddTransient<OneLoginService>();

        return services;
    }
}
