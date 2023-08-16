#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmServiceClientProvider
{
    IOrganizationServiceAsync2 GetClient(string name);
}

public class CrmServiceClientProvider : ICrmServiceClientProvider
{
    private readonly IOptionsMonitor<CrmServiceClientOptions> _options;

    public CrmServiceClientProvider(IOptionsMonitor<CrmServiceClientOptions> options)
    {
        _options = options;
    }

    public IOrganizationServiceAsync2 GetClient(string name)
    {
        return _options.Get(name).ServiceClient!;
    }
}

public class CrmServiceClientOptions
{
    public ServiceClient? ServiceClient { get; set; }
}

file class PostConfigureCrmServiceClientOptions : IConfigureNamedOptions<CrmServiceClientOptions>
{
    private readonly string _name;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, ServiceClient> _createServiceClient;

    public PostConfigureCrmServiceClientOptions(
        string name,
        IServiceProvider serviceProvider,
        Func<IServiceProvider, ServiceClient> createServiceClient)
    {
        _name = name;
        _serviceProvider = serviceProvider;
        _createServiceClient = createServiceClient;
    }

    public void Configure(string? name, CrmServiceClientOptions options)
    {
        if (name == _name)
        {
            options.ServiceClient = _createServiceClient(_serviceProvider);
        }
    }

    public void Configure(CrmServiceClientOptions options) => Configure(Options.DefaultName, options);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceClient(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, ServiceClient> createServiceClient)
    {
        services.TryAddSingleton<ICrmServiceClientProvider, CrmServiceClientProvider>();

        services.AddOptions<CrmServiceClientOptions>(name);

        services.AddSingleton<IConfigureOptions<CrmServiceClientOptions>>(
            sp => new PostConfigureCrmServiceClientOptions(name, sp, createServiceClient));

        return services;
    }
}
