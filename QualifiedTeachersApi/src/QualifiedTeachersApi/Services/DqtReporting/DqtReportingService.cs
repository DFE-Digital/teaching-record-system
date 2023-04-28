using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.CrmEntityChanges;

namespace QualifiedTeachersApi.Services.DqtReporting;

public class DqtReportingService : BackgroundService
{
    private const string ChangesKey = "DqtReporting";
    private const string IdColumnName = "Id";

    private readonly DqtReportingOptions _options;
    private readonly ICrmEntityChangesService _crmEntityChangesService;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ILogger<DqtReportingService> _logger;
    private readonly Dictionary<string, EntityMetadata> _entityMetadata = new();

    public DqtReportingService(
        IOptions<DqtReportingOptions> optionsAccessor,
        ICrmEntityChangesService crmEntityChangesService,
        IDataverseAdapter dataverseAdapter,
        ILogger<DqtReportingService> logger)
    {
        _options = optionsAccessor.Value;
        _crmEntityChangesService = crmEntityChangesService;
        _dataverseAdapter = dataverseAdapter;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadEntityMetadata();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessChanges(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed polling for entity changes.");
            }
        }
    }

    internal async Task LoadEntityMetadata()
    {
        foreach (var entity in _options.Entities)
        {
            var entityMetadata = await _dataverseAdapter.GetEntityMetadata(entity, EntityFilters.Default | EntityFilters.Attributes);

            if (entityMetadata.ChangeTrackingEnabled != true)
            {
                throw new Exception($"Entity '{entity}' does not have change tracking enabled.");
            }

            _entityMetadata[entity] = entityMetadata;
        }
    }

    internal Task ProcessChanges(CancellationToken cancellationToken) =>
        Parallel.ForEachAsync(
            _options.Entities,
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.ProcessAllEntityTypesConcurrently ? _options.Entities.Length : 1,
                CancellationToken = cancellationToken
            },
            (entityType, ct) => new ValueTask(ProcessChangesForEntityType(entityType, ct)));

    internal async Task ProcessChangesForEntityType(string entityLogicalName, CancellationToken cancellationToken)
    {
        await foreach (var changes in _crmEntityChangesService.GetEntityChanges(ChangesKey, entityLogicalName).WithCancellation(cancellationToken))
        {
            var newOrUpdatedItems = new List<NewOrUpdatedItem>();
            var removedOrDeletedItems = new List<RemovedOrDeletedItem>();

            foreach (var change in changes)
            {
                if (change is NewOrUpdatedItem newOrUpdatedItem)
                {
                    newOrUpdatedItems.Add(newOrUpdatedItem);
                }
                else if (change is RemovedOrDeletedItem removedOrDeletedItem)
                {
                    removedOrDeletedItems.Add(removedOrDeletedItem);
                }
                else
                {
                    throw new Exception($"Received unknown change type: '{change.GetType().Name}'.");
                }
            }

            await HandleNewOrUpdatedItems(newOrUpdatedItems, cancellationToken);
            await HandleRemovedOrDeletedItems(removedOrDeletedItems, cancellationToken);
        }
    }

    private async Task HandleNewOrUpdatedItems(
        IReadOnlyCollection<NewOrUpdatedItem> newOrUpdatedItems,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (newOrUpdatedItems.Count == 0)
        {
            return;
        }

        var entityLogicalName = newOrUpdatedItems.First().NewOrUpdatedEntity.LogicalName;
        var entities = newOrUpdatedItems.Select(e => e.NewOrUpdatedEntity);

        foreach (var entity in entities)
        {
            var command = CreateUpsertRowCommand(entity);
            await ExecuteCommand(command, cancellationToken);
        }
    }

    private async Task HandleRemovedOrDeletedItems(
        IReadOnlyCollection<RemovedOrDeletedItem> removedOrDeletedItems,
        CancellationToken cancellationToken)
    {
        if (removedOrDeletedItems.Count == 0)
        {
            return;
        }

        var entityLogicalName = removedOrDeletedItems.First().RemovedItem.LogicalName;
        var ids = removedOrDeletedItems.Select(e => e.RemovedItem.Id).ToArray();

        var command = CreateDeleteRowsCommand(entityLogicalName, ids);
        await ExecuteCommand(command, cancellationToken);
    }

    private async Task ExecuteCommand(SqlCommand command, CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(_options.ReportingDbConnectionString);
        await conn.OpenAsync();

        command.Connection = conn;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private SqlCommand CreateDeleteRowsCommand(string entityLogicalName, IReadOnlyCollection<Guid> ids)
    {
        var tableName = entityLogicalName;
        var idParameters = ids.Select((id, i) => new SqlParameter($"@id{i}", id)).ToArray();

        var command = new SqlCommand(
            $"delete from [{tableName}] where [{IdColumnName}] in ({string.Join(", ", idParameters.Select(p => $"{p.ParameterName}"))})");

        command.Parameters.AddRange(idParameters);

        return command;
    }

    private SqlCommand CreateUpsertRowCommand(Entity entity)
    {
        var entityMetadata = _entityMetadata[entity.LogicalName];
        var tableName = entityMetadata.LogicalName;
        var primaryKeyColumnName = entityMetadata.PrimaryIdAttribute;

        var sortedAttrs = entityMetadata.Attributes.OrderBy(a => a.ColumnNumber).Select(a => a.LogicalName).ToArray();

        var parametersAndColumns = entity.Attributes
            .OrderBy(a => Array.IndexOf(sortedAttrs, a.Key))
            .Select((a, i) => (
                ColumnName: a.Key == primaryKeyColumnName ? IdColumnName : a.Key,
                Parameter: new SqlParameter($"@p{i}", MapCrmAttributeValueToParameterValue(a.Value))))
            .ToArray();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine($"merge into [{tableName}] as target");
        sqlBuilder.AppendLine("using (select");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Select(a => $"\t{a.Parameter.ParameterName} as [{a.ColumnName}]"));
        sqlBuilder.AppendLine("\n)as source");
        sqlBuilder.AppendLine($"on source.[{IdColumnName}] = target.[{IdColumnName}]");
        sqlBuilder.AppendLine("when matched then update set");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Where(p => p.ColumnName != IdColumnName).Select(p => $"\t[{p.ColumnName}] = {p.Parameter.ParameterName}"));
        sqlBuilder.AppendLine("\nwhen not matched then insert (");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Select(p => $"\t{p.ColumnName}"));
        sqlBuilder.AppendLine("\n) values (");
        sqlBuilder.AppendJoin(",\n", parametersAndColumns.Select(p => $"\t{p.Parameter.ParameterName}"));
        sqlBuilder.AppendLine("\n);");

        var command = new SqlCommand(sqlBuilder.ToString());
        command.Parameters.AddRange(parametersAndColumns.Select(p => p.Parameter).ToArray());

        return command;

        static object MapCrmAttributeValueToParameterValue(object value) => value switch
        {
            OptionSetValue optionSetValue => optionSetValue.Value,
            EntityReference entityReference => entityReference.Id,
            Money money => money.Value,
            _ => value
        };
    }
}
