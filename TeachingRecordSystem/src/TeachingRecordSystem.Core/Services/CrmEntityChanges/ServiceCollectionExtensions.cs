using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.CrmEntityChanges;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrmEntityChanges(this IServiceCollection services)
    {
        services.AddSingleton<ICrmEntityChangesService, CrmEntityChangesService>();
        services.TryAddSingleton<ICrmServiceClientProvider, CrmServiceClientProvider>();

        return services;
    }
}
