using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Users;

public static class Extensions
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddTransient<UserService>();

        return services;
    }
}
