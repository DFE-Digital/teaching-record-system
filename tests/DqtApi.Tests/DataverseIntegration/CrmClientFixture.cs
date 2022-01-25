using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace DqtApi.Tests.DataverseIntegration
{
    public sealed class CrmClientFixture : IAsyncDisposable
    {
        private readonly List<(string EntityName, Guid EntityId)> _createdEntities;

        public CrmClientFixture()
        {
            _createdEntities = new();
            Clock = new();
            Configuration = GetConfiguration();
            ServiceClient = GetCrmServiceClient();
        }

        public TestableClock Clock { get; }

        public IConfiguration Configuration { get; }

        public ServiceClient ServiceClient { get; }

        public async Task CleanupEntities()
        {
            await Task.WhenAll(_createdEntities.Select(e =>
            {
                var deactivateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference(e.EntityName, e.EntityId),
                    State = new OptionSetValue((int)ContactState.Inactive),
                    Status = new OptionSetValue(2)
                };

                return ServiceClient.ExecuteAsync(deactivateRequest);
            }));

            _createdEntities.Clear();
        }

        public DataverseAdapter CreateDataverseAdapter() => new(ServiceClient, Clock, new MemoryCache(Options.Create<MemoryCacheOptions>(new())));

        public Task InitializeAsync() => Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            await CleanupEntities();
            ServiceClient.Dispose();
        }

        public void RegisterForCleanup(Entity entity)
        {
            _createdEntities.Add((entity.LogicalName, entity.Id));
        }

        public void RegisterForCleanup(string entityName, Guid entityId)
        {
            if (entityId == Guid.Empty)
            {
                return;
            }

            _createdEntities.Add((entityName, entityId));
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
