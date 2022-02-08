using System;
using System.Collections.Generic;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DqtApi.Tests.DataverseIntegration
{
    public class EntityCleanupHelper
    {
        private readonly List<(string EntityName, Guid EntityId)> _createdEntities;
        private readonly ServiceClient _serviceClient;

        public EntityCleanupHelper(ServiceClient serviceClient)
        {
            _createdEntities = new();
            _serviceClient = serviceClient;
        }

        public async Task CleanupEntities()
        {
            var multiRequest = new ExecuteMultipleRequest()
            {
                Requests = new(),
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true
                }
            };

            (string EntityName, Guid EntityId)[] toBeCleared;
            lock (_createdEntities)
            {
                toBeCleared = _createdEntities.ToArray();
                _createdEntities.Clear();
            }

            foreach (var (entityName, entityId) in toBeCleared)
            {
                multiRequest.Requests.Add(new DeleteRequest()
                {
                    Target = new EntityReference(entityName, entityId)
                });
            }

            await _serviceClient.ExecuteAsync(multiRequest);
        }

        public void RegisterForCleanup(Entity entity)
        {
            lock (_createdEntities)
            {
                _createdEntities.Add((entity.LogicalName, entity.Id));
            }
        }

        public void RegisterForCleanup(string entityName, Guid entityId)
        {
            if (entityId == Guid.Empty)
            {
                return;
            }

            lock (_createdEntities)
            {
                _createdEntities.Add((entityName, entityId));
            }
        }
    }
}
