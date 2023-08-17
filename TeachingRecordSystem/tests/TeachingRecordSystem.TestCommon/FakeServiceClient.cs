using System.Reflection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.TestCommon;

public class FakeServiceClient : IOrganizationServiceAsync
{
    private readonly object _lock = new();
    private DataSnapshot _data = new();

    public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        throw new NotSupportedException();
    }

    public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        throw new NotSupportedException();
    }

    public Guid Create(Entity entity)
    {
        lock (_lock)
        {
            _data = Create(_data, entity, out var id);
            return id;
        }
    }

    public Task<Guid> CreateAsync(Entity entity) =>
        Task.FromResult(Create(entity));

    public void Delete(string entityName, Guid id)
    {
        lock (_lock)
        {
            _data = Delete(_data, entityName, id);
        }
    }

    public Task DeleteAsync(string entityName, Guid id)
    {
        Delete(entityName, id);
        return Task.CompletedTask;
    }

    public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        throw new NotSupportedException();
    }

    public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
    {
        throw new NotSupportedException();
    }

    public OrganizationResponse Execute(OrganizationRequest request)
    {
        lock (_lock)
        {
            _data = Execute(_data, request, out var response);
            return response;
        }
    }

    public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request) =>
        Task.FromResult(Execute(request));

    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => Retrieve(_data, entityName, id, columnSet);

    public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet) =>
        Task.FromResult(Retrieve(entityName, id, columnSet));

    public EntityCollection RetrieveMultiple(QueryBase query) => RetrieveMultiple(_data, query);

    public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query) =>
        Task.FromResult(RetrieveMultiple(query));

    public void Update(Entity entity)
    {
        lock (_lock)
        {
            _data = Update(_data, entity);
        }
    }

    public Task UpdateAsync(Entity entity)
    {
        Update(entity);
        return Task.CompletedTask;
    }

    private static DataSnapshot Execute(DataSnapshot currentSnapshot, OrganizationRequest request, out OrganizationResponse response)
    {
        if (request is RetrieveMultipleRequest retrieveMultipleRequest)
        {
            response = new RetrieveMultipleResponse()
            {
                Results =
                {
                    { nameof(EntityCollection), RetrieveMultiple(currentSnapshot, retrieveMultipleRequest.Query) }
                }
            };

            return currentSnapshot;
        }

        if (request is CreateRequest createRequest)
        {
            var newSnapshot = Create(currentSnapshot, createRequest.Target, out var id);

            response = new CreateResponse()
            {
                Results =
                {
                    { "id", id }
                }
            };

            return newSnapshot;
        }

        if (request is UpdateRequest updateRequest)
        {
            var newSnapshot = Update(currentSnapshot, updateRequest.Target);

            response = new UpdateResponse();

            return newSnapshot;
        }

        if (request is DeleteRequest deleteRequest)
        {
            var newSnapshot = Delete(currentSnapshot, deleteRequest.Target.LogicalName, deleteRequest.Target.Id);

            response = new DeleteResponse();

            return newSnapshot;
        }

        if (request is ExecuteTransactionRequest executeTransactionRequest)
        {
            var responseCollection = new OrganizationResponseCollection();

            var snapshot = currentSnapshot;
            foreach (var innerRequest in executeTransactionRequest.Requests)
            {
                snapshot = Execute(snapshot, innerRequest, out var innerResponse);
                responseCollection.Add(innerResponse);
            }

            response = new ExecuteTransactionResponse()
            {
                Results =
                {
                    { nameof(ExecuteTransactionResponse.Responses), responseCollection }
                }
            };

            return snapshot;
        }

        throw new NotImplementedException($"Support for {request.GetType().Name} requests is not implemented.");
    }

    private static DataSnapshot Create(DataSnapshot currentSnapshot, Entity entity, out Guid id)
    {
        ThrowOnUnsupportData(entity);

        if (!TryGetId(entity, out id))
        {
            id = Guid.NewGuid();
        }

        var attributes = entity.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!attributes.ContainsKey("statecode"))
        {
            attributes.Add("statecode", 0);
        }

        return currentSnapshot.Add(new(entity.LogicalName, id, attributes));
    }

    private static Entity Retrieve(DataSnapshot currentSnapshot, string entityName, Guid id, ColumnSet columnSet)
    {
        return currentSnapshot.Entities.SingleOrDefault(e => e.Id == id && e.EntityName == entityName)?.ToEntity(columnSet) ??
            throw new ArgumentException("Could not retrieve entity.");
    }

    private static EntityCollection RetrieveMultiple(DataSnapshot currentSnapshot, QueryBase query)
    {
        if (query is QueryByAttribute queryByAttribute)
        {
            if (queryByAttribute.TopCount.HasValue)
            {
                throw new NotSupportedException();
            }

            var filter = queryByAttribute.Attributes.Zip(queryByAttribute.Values, (attr, value) => (AttributeName: attr, AttributeValue: value))
                .Aggregate(
                    (Predicate<EntitySnapshot>)(e => e.EntityName == queryByAttribute.EntityName),
                    (acc, attr) => e => acc(e) && Equals(e.Attributes.GetValueOrDefault(attr.AttributeName), attr.AttributeValue));

            return HandleQuery(filter, queryByAttribute.PageInfo, queryByAttribute.EntityName, queryByAttribute.ColumnSet, queryByAttribute.Orders);
        }

        if (query is QueryExpression queryExpression)
        {
            if (queryExpression.TopCount.HasValue)
            {
                throw new NotSupportedException();
            }

            if (queryExpression.LinkEntities.Count > 0)
            {
                throw new NotImplementedException();
            }

            if (queryExpression.SubQueryExpression is not null)
            {
                throw new NotSupportedException();
            }

            if (queryExpression.Distinct)
            {
                throw new NotSupportedException();
            }

            Predicate<EntitySnapshot> filter = e => e.EntityName == queryExpression.EntityName && CreateFilter(queryExpression.Criteria)(e);

            return HandleQuery(filter, queryExpression.PageInfo, queryExpression.EntityName, queryExpression.ColumnSet, queryExpression.Orders);

            static Predicate<EntitySnapshot> CreateFilter(FilterExpression filterExpression)
            {
                var conditionsPredicate = filterExpression.Conditions.Aggregate(
                    (Predicate<EntitySnapshot>)(e => true),
                    (acc, cond) =>
                    {
                        Predicate<EntitySnapshot> thisConditionPredicate = cond.Operator switch
                        {
                            ConditionOperator.Equal => e => Equals(e.Attributes.GetValueOrDefault(cond.AttributeName), cond.Values.Single()),
                            ConditionOperator.NotNull => e => e.Attributes.GetValueOrDefault(cond.AttributeName) is not null,
                            ConditionOperator.Null => e => e.Attributes.GetValueOrDefault(cond.AttributeName) is null,
                            _ => throw new NotImplementedException($"Support for the {cond.Operator} operator is not implemented.")
                        };

                        return e => filterExpression.FilterOperator == LogicalOperator.And ?
                            acc(e) && thisConditionPredicate(e) :
                            acc(e) || thisConditionPredicate(e);
                    });

                var filtersPredicate = filterExpression.Filters.Aggregate(
                    (Predicate<EntitySnapshot>)(e => true),
                    (acc, filter) =>
                    {
                        var thisFilterPredicate = CreateFilter(filter);

                        return e => filterExpression.FilterOperator == LogicalOperator.And ?
                            acc(e) && thisFilterPredicate(e) :
                            acc(e) || thisFilterPredicate(e);
                    });

                return e => conditionsPredicate(e) && filtersPredicate(e);
            }
        }

        EntityCollection HandleQuery(
            Predicate<EntitySnapshot> filter,
            PagingInfo pageInfo,
            string entityName,
            ColumnSet columnSet,
            DataCollection<OrderExpression> orders)
        {
            var results = ApplyOrders(currentSnapshot.Entities.Where(e => filter(e)))
                .Select(s => s.ToEntity(columnSet))
                .ToList();

            var totalRecordCount = results.Count;

            if (pageInfo?.PageNumber is int pageNumber && pageNumber > 1)
            {
                throw new NotImplementedException();
            }

            if (pageInfo?.Count is int count and > 0)
            {
                results = results.Take(count).ToList();
            }

            var entityCollection = new EntityCollection(results)
            {
                EntityName = entityName,
                MoreRecords = results.Count < totalRecordCount
            };

            if (pageInfo?.ReturnTotalRecordCount == true)
            {
                entityCollection.TotalRecordCount = totalRecordCount;
            }

            return entityCollection;

            IEnumerable<EntitySnapshot> ApplyOrders(IEnumerable<EntitySnapshot> results)
            {
                foreach (var order in orders)
                {
                    results = order.OrderType == OrderType.Ascending ?
                        results.OrderBy(e => e.Attributes.GetValueOrDefault(order.AttributeName)) :
                        results.OrderByDescending(e => e.Attributes.GetValueOrDefault(order.AttributeName));
                }

                return results;
            }
        }

        throw new NotImplementedException();
    }

    private static DataSnapshot Update(DataSnapshot currentSnapshot, Entity entity)
    {
        ThrowOnUnsupportData(entity);

        if (!TryGetId(entity, out var id))
        {
            throw new ArgumentException("Could not determine entity ID.", nameof(entity));
        }

        return currentSnapshot.Update(entity.LogicalName, id, e =>
        {
            var newAttributes = e.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var attr in entity.Attributes)
            {
                newAttributes[attr.Key] = attr.Value;
            }

            return new(e.EntityName, e.Id, newAttributes);
        });
    }

    private static DataSnapshot Delete(DataSnapshot currentSnapshot, string entityName, Guid id)
    {
        var newSnapshot = currentSnapshot.Remove(entityName, id, out var removed);

        if (!removed)
        {
            throw new ArgumentException("Entity does not exist.");
        }

        return newSnapshot;
    }

    private static string GetPrimaryIdAttribute(Type entityType)
    {
        var fieldInfo = entityType.GetField("PrimaryIdAttribute", BindingFlags.Public | BindingFlags.Static) ??
            throw new ArgumentException("Invalid entity type.", nameof(entityType));

        return (string)fieldInfo.GetValue(obj: null)!;
    }

    private static void ThrowOnUnsupportData(Entity entity)
    {
        if (entity.KeyAttributes.Count > 0)
        {
            throw new NotSupportedException($"{nameof(entity.KeyAttributes)} are not supported.");
        }

        if (entity.RelatedEntities.Count > 0)
        {
            throw new NotSupportedException($"{nameof(entity.RelatedEntities)} are not supported.");
        }

        if (entity.FormattedValues.Count > 0)
        {
            throw new NotSupportedException($"{nameof(entity.FormattedValues)} are not supported.");
        }

        if (!string.IsNullOrEmpty(entity.RowVersion))
        {
            throw new NotSupportedException($"{nameof(entity.RowVersion)} is not supported.");
        }

        if (entity.KeyAttributes.Count > 0)
        {
            throw new NotSupportedException($"{nameof(entity.KeyAttributes)} are not supported.");
        }

        if (entity.ExtensionData is not null)
        {
            throw new NotSupportedException($"{nameof(entity.ExtensionData)} is not supported.");
        }
    }

    private static bool TryGetId(Entity entity, out Guid id)
    {
        if (entity.Id != Guid.Empty)
        {
            id = entity.Id;
            return true;
        }

        var primaryIdAttribute = GetPrimaryIdAttribute(entity.GetType());

        if (entity.Attributes.TryGetValue(primaryIdAttribute, out var primaryAttributeObj))
        {
            id = (Guid)primaryAttributeObj;
            return true;
        }

        id = default;
        return false;
    }

    private class DataSnapshot
    {
        private readonly EntitySnapshot[] _entities;

        public DataSnapshot()
            : this(Enumerable.Empty<EntitySnapshot>())
        {
        }

        public DataSnapshot(IEnumerable<EntitySnapshot> entities)
        {
            _entities = entities.ToArray();
        }

        public IReadOnlyCollection<EntitySnapshot> Entities => _entities;

        public DataSnapshot Add(EntitySnapshot entity)
        {
            if (_entities.Any(e => e.Id == entity.Id && e.EntityName == entity.EntityName))
            {
                throw new ArgumentException("Entity already exists in snapshot.");
            }

            return new DataSnapshot(_entities.Append(entity));
        }

        public DataSnapshot Update(string entityName, Guid id, Func<EntitySnapshot, EntitySnapshot> updateSnapshot)
        {
            var entity = _entities.SingleOrDefault(e => e.Id == id && e.EntityName == entityName) ??
                throw new ArgumentException("Entity does not exist in snapshot.");

            var newEntity = updateSnapshot(entity);

            return new(_entities.Where(e => e != entity).Append(newEntity));
        }

        public DataSnapshot Remove(string entityName, Guid id, out bool removed)
        {
            var withoutEntity = _entities.Where(e => !(e.EntityName == entityName && e.Id == id)).ToArray();
            removed = withoutEntity.Length < _entities.Length;
            return new(withoutEntity);
        }
    }

    private record EntitySnapshot(string EntityName, Guid Id, IReadOnlyDictionary<string, object> Attributes)
    {
        public Entity ToEntity() => ToEntity(new ColumnSet(allColumns: true));

        public Entity ToEntity(ColumnSet? columnSet)
        {
            columnSet ??= new();

            var entity = new Entity(EntityName, Id);

            foreach (var kvp in Attributes)
            {
                if (columnSet.AllColumns || columnSet.Columns.Contains(kvp.Key))
                {
                    entity.Attributes.Add(kvp.Key, kvp.Value);
                }
            }

            return entity;
        }
    }
}
