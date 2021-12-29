using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class CrmClientFixture : IDisposable, IAsyncLifetime
    {
        private readonly List<(string EntityName, Guid EntityId)> _createdEntities;

        public CrmClientFixture()
        {
            _createdEntities = new();
            Configuration = GetConfiguration();
            ServiceClient = GetCrmServiceClient();
        }

        public IConfiguration Configuration { get; }

        public ServiceClient ServiceClient { get; }

        public DataverseAdaptor CreateDataverseAdaptor() => new(ServiceClient);

        public Task InitializeAsync() => Task.CompletedTask;

        public virtual void Dispose()
        {
            ServiceClient.Dispose();
        }

        public Task DisposeAsync() => Task.WhenAll(_createdEntities.Select(e =>
        {
            var deactivateRequest = new SetStateRequest()
            {
                EntityMoniker = new EntityReference(e.EntityName, e.EntityId),
                State = new OptionSetValue((int)ContactState.Inactive),
                Status = new OptionSetValue(2)
            };

            return ServiceClient.ExecuteAsync(deactivateRequest);
        }));

        public void RegisterForCleanup(Entity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException("Entity ID is not set.", nameof(entity));
            }

            _createdEntities.Add((entity.LogicalName, entity.Id));
        }

        public void RegisterForCleanup(string entityName, Guid entityId)
        {
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
