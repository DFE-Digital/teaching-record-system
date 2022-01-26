using System;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace DqtApi.Tests.DataverseIntegration
{
    public sealed class CrmClientFixture : IAsyncDisposable
    {
        private readonly EntityCleanupHelper _createdEntityTracker;

        public CrmClientFixture()
        {
            Clock = new();
            Configuration = GetConfiguration();
            ServiceClient = GetCrmServiceClient();
            _createdEntityTracker = CreateEntityCleanupHelper();
        }

        public TestableClock Clock { get; }

        public IConfiguration Configuration { get; }

        public ServiceClient ServiceClient { get; }

        public DataverseAdapter CreateDataverseAdapter() => new(ServiceClient, Clock, new MemoryCache(Options.Create<MemoryCacheOptions>(new())));

        public EntityCleanupHelper CreateEntityCleanupHelper() => new(ServiceClient);

        public Task InitializeAsync() => Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            await _createdEntityTracker.CleanupEntities();
            ServiceClient.Dispose();
        }

        public void RegisterForCleanup(Entity entity) =>
            _createdEntityTracker.RegisterForCleanup(entity);

        public void RegisterForCleanup(string entityName, Guid entityId) =>
            _createdEntityTracker.RegisterForCleanup(entityName, entityId);

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .AddUserSecrets<CrmClientFixture>(optional: true)
                .AddEnvironmentVariables("IntegrationTests_")
                .Build();

        // This is wrapped up in Task.Run because the ServiceClient constructor can deadlock in some environments (e.g. CI).
        // InitServiceAsync().Result within Microsoft.PowerPlatform.Dataverse.Client.ConnectionService.GetCachedService() looks to be the culprit
        private ServiceClient GetCrmServiceClient() => Task.Run(() =>
            new ServiceClient(
                new Uri(Configuration["CrmUrl"]),
                Configuration["CrmClientId"],
                Configuration["CrmClientSecret"],
                useUniqueInstance: true)).Result;
    }
}
