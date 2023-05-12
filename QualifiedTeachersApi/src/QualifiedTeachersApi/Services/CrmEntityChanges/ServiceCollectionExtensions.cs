﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using QualifiedTeachersApi.DataStore.Crm;

namespace QualifiedTeachersApi.Services.CrmEntityChanges;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrmEntityChanges(this IServiceCollection services)
    {
        services.AddSingleton<ICrmEntityChangesService, CrmEntityChangesService>();
        services.TryAddSingleton<ICrmServiceClientProvider, CrmServiceClientProvider>();

        return services;
    }
}
