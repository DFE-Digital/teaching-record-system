using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public static class Extensions
{
    public static IServiceCollection AddOneLoginService(this IServiceCollection services)
    {
        services.AddTransient<OneLoginService>();

        return services;
    }

    public static IServiceCollection AddIdDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdDbContext>(options => options.UseNpgsql(configuration.GetRequiredConnectionString("Id")));

        return services;
    }
}
