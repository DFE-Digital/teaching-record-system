using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrmEntityChanges(this IServiceCollection services)
    {
        services.AddSingleton<ICrmEntityChangesService, CrmEntityChangesService>();
        services.TryAddSingleton<ICrmServiceClientProvider, CrmServiceClientProvider>();

        return services;
    }
}
