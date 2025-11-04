using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.PowerPlatform.Dataverse.Client;

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
