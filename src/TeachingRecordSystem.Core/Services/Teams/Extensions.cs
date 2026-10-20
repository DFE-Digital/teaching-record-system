using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Teams;

public static class Extensions
{
    public static IServiceCollection AddTeamService(this IServiceCollection services)
    {
        services.AddTransient<TeamService>();

        return services;
    }
}
