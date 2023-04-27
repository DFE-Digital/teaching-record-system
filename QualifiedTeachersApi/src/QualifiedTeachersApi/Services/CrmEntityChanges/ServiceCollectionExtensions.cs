using Microsoft.Extensions.DependencyInjection;

namespace QualifiedTeachersApi.Services.CrmEntityChanges;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrmEntityChanges(this IServiceCollection services)
    {
        services.AddSingleton<ICrmEntityChangesService, DataverseCrmEntityChangesService>();

        return services;
    }
}
