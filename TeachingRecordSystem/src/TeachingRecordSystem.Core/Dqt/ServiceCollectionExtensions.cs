using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Services.CrmEntityChanges;

namespace TeachingRecordSystem.Core.Dqt;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultServiceClient(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        Func<IServiceProvider, IOrganizationServiceAsync2> getServiceClient)
    {
        services.AddServiceClient(name: null, lifetime, getServiceClient);

        return services;
    }

    public static IServiceCollection AddNamedServiceClient(
        this IServiceCollection services,
        string name,
        ServiceLifetime lifetime,
        Func<IServiceProvider, IOrganizationServiceAsync2> getServiceClient)
    {
        return AddServiceClient(services, name, lifetime, getServiceClient);
    }

    public static IServiceCollection AddCrmEntityChangesService(
        this IServiceCollection services,
        string name)
    {
        // Since ICrmEntityChangesService is registered as a singleton, the corresponding ServiceClient should be too
        var orgServiceRegistration = services.SingleOrDefault(sd => sd.ServiceKey as string == name && sd.ServiceType == typeof(IOrganizationServiceAsync));
        if (orgServiceRegistration is null)
        {
            throw new InvalidOperationException($"No ServiceClient has been registered for name: '{name}'.");
        }
        if (orgServiceRegistration.Lifetime != ServiceLifetime.Singleton)
        {
            throw new InvalidOperationException(
                $"The ServiceClient used for {nameof(CrmEntityChangesService)} should be registered with {nameof(ServiceLifetime.Singleton)} lifetime.");
        }

        services.AddKeyedSingleton<ICrmEntityChangesService>(
            name,
            (IServiceProvider serviceProvider, object? key) =>
            {
                var namedOrgService = serviceProvider.GetRequiredKeyedService<IOrganizationServiceAsync>(name);
                return ActivatorUtilities.CreateInstance<CrmEntityChangesService>(serviceProvider, namedOrgService);
            });

        return services;
    }

    private static IServiceCollection AddServiceClient(
        this IServiceCollection services,
        string? name,
        ServiceLifetime lifetime,
        Func<IServiceProvider, IOrganizationServiceAsync2> getServiceClient)
    {
        services.TryAddSingleton<ICrmServiceClientProvider, CrmServiceClientProvider>();

        services.Add(ServiceDescriptor.DescribeKeyed(
            typeof(IOrganizationServiceAsync2),
            name,
            implementationFactory: (IServiceProvider serviceProvider, object? key) => getServiceClient(serviceProvider),
            lifetime));

        services.Add(ServiceDescriptor.DescribeKeyed(
            typeof(IOrganizationServiceAsync),
            name,
            implementationFactory: (IServiceProvider serviceProvider, object? key) => serviceProvider.GetRequiredKeyedService<IOrganizationServiceAsync2>(key),
            lifetime));

        return services;
    }
}
