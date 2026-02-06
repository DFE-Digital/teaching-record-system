using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public static class Extensions
{
    public static IServiceCollection AddOneLoginService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<OneLoginService>();

        services.AddDbContext<IdDbContext>(options => options.UseNpgsql(configuration.GetRequiredConnectionString("Id")));

        return services;
    }
}
