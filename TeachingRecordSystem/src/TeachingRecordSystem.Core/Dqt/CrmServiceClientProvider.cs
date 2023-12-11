using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmServiceClientProvider
{
    IOrganizationServiceAsync2 GetClient(string name);
}

public class CrmServiceClientProvider(IServiceProvider serviceProvider) : ICrmServiceClientProvider
{
    public IOrganizationServiceAsync2 GetClient(string name) =>
        serviceProvider.GetRequiredKeyedService<IOrganizationServiceAsync2>(name);
}
