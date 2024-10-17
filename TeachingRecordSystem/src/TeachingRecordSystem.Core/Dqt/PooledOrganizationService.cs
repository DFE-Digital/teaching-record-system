using System.Collections.Concurrent;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt;

internal sealed class PooledOrganizationService : IOrganizationServiceAsync2, IDisposable
{
    private readonly BlockingCollection<ServiceClient> _pool;

    private PooledOrganizationService(IEnumerable<ServiceClient> connections)
    {
        _pool = new BlockingCollection<ServiceClient>(new ConcurrentQueue<ServiceClient>(connections));
    }

    public static PooledOrganizationService Create(ServiceClient serviceClient, int size)
    {
        var connections = new List<ServiceClient>(size);
        connections.AddRange(Enumerable.Range(0, size).Select(_ => serviceClient.Clone()));
        return new PooledOrganizationService(connections);
    }

    public void Dispose()
    {
        _pool.Dispose();
    }

    private async Task<TResult> WithPooledConnectionAsync<TResult>(Func<IOrganizationServiceAsync2, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        var client = _pool.Take(cancellationToken);
        try
        {
            return await action(client);
        }
        finally
        {
            _pool.Add(client);
        }
    }

    private async Task WithPooledConnectionAsync(Func<IOrganizationServiceAsync2, Task> action, CancellationToken cancellationToken = default) =>
        await WithPooledConnectionAsync(async client =>
        {
            await action(client);
            return 1;
        });

    private TResult WithPooledConnection<TResult>(Func<IOrganizationServiceAsync2, TResult> action)
    {
        var client = _pool.Take();
        try
        {
            return action(client);
        }
        finally
        {
            _pool.Add(client);
        }
    }

    private void WithPooledConnection(Action<IOrganizationServiceAsync2> action) =>
        WithPooledConnection(client =>
        {
            action(client);
            return 1;
        });

    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.AssociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken), cancellationToken);

    public Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.CreateAsync(entity, cancellationToken), cancellationToken);

    public Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.CreateAndReturnAsync(entity, cancellationToken), cancellationToken);

    public Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.DeleteAsync(entityName, id, cancellationToken), cancellationToken);

    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.DisassociateAsync(entityName, entityId, relationship, relatedEntities, cancellationToken), cancellationToken);

    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.ExecuteAsync(request, cancellationToken), cancellationToken);

    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.RetrieveAsync(entityName, id, columnSet, cancellationToken), cancellationToken);

    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.RetrieveMultipleAsync(query, cancellationToken), cancellationToken);

    public Task UpdateAsync(Entity entity, CancellationToken cancellationToken) =>
        WithPooledConnectionAsync(s => s.UpdateAsync(entity, cancellationToken), cancellationToken);

    public Task<Guid> CreateAsync(Entity entity) =>
        WithPooledConnectionAsync(s => s.CreateAsync(entity));

    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet) =>
        WithPooledConnectionAsync(s => s.RetrieveAsync(entityName, id, columnSet));

    public Task UpdateAsync(Entity entity) =>
        WithPooledConnectionAsync(s => s.UpdateAsync(entity));

    public Task DeleteAsync(string entityName, Guid id) =>
        WithPooledConnectionAsync(s => s.DeleteAsync(entityName, id));

    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request) =>
        WithPooledConnectionAsync(s => s.ExecuteAsync(request));

    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
        WithPooledConnectionAsync(s => s.AssociateAsync(entityName, entityId, relationship, relatedEntities));

    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
        WithPooledConnectionAsync(s => s.DisassociateAsync(entityName, entityId, relationship, relatedEntities));

    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query) =>
        WithPooledConnectionAsync(s => s.RetrieveMultipleAsync(query));

    public Guid Create(Entity entity) =>
        WithPooledConnection(s => s.Create(entity));

    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) =>
        WithPooledConnection(s => s.Retrieve(entityName, id, columnSet));

    public void Update(Entity entity) =>
        WithPooledConnection(s => s.Update(entity));

    public void Delete(string entityName, Guid id) =>
        WithPooledConnection(s => s.Delete(entityName, id));

    public OrganizationResponse Execute(OrganizationRequest request) =>
        WithPooledConnection(s => s.Execute(request));

    public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
        WithPooledConnection(s => s.Associate(entityName, entityId, relationship, relatedEntities));

    public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
        WithPooledConnection(s => s.Disassociate(entityName, entityId, relationship, relatedEntities));

    public EntityCollection RetrieveMultiple(QueryBase query) =>
        WithPooledConnection(s => s.RetrieveMultiple(query));
}
