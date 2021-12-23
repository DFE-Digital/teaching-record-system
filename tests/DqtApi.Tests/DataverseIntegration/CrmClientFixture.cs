using System;
using DqtApi.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DqtApi.Tests.DataverseIntegration
{
    public class CrmClientFixture : IDisposable
    {
        public CrmClientFixture()
        {
            Configuration = GetConfiguration();
            ServiceClient = GetCrmServiceClient();
        }

        public IConfiguration Configuration { get; }

        public ServiceClient ServiceClient { get; }

        public DataverseAdaptor CreateDataverseAdaptor() => new(ServiceClient);

        public virtual void Dispose()
        {
            ServiceClient.Dispose();
        }

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .AddUserSecrets<CrmClientFixture>(optional: true)
                .AddEnvironmentVariables("IntegrationTests_")
                .Build();

        private ServiceClient GetCrmServiceClient() =>
            new(
                new Uri(Configuration["CrmUrl"]),
                Configuration["CrmClientId"],
                Configuration["CrmClientSecret"],
                useUniqueInstance: true);
    }
}
