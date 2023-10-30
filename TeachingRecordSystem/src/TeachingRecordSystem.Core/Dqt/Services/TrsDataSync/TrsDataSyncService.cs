using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;

namespace TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;

public class TrsDataSyncService : BackgroundService
{
    public const string CrmClientName = "TrsDataSync";
    public const string ChangesKey = "TrsDataSync";

    private const int MaxEntityTypesToProcessConcurrently = 10;
    private const int PageSize = 1000;

    private readonly ICrmEntityChangesService _crmEntityChangesService;
    private readonly TrsDataSyncHelper _trsDataSyncHelper;
    private readonly ILogger<TrsDataSyncService> _logger;
    private readonly TrsDataSyncServiceOptions _options;

    public TrsDataSyncService(
        ICrmEntityChangesService crmEntityChangesService,
        TrsDataSyncHelper trsDataSyncHelper,
        IOptions<TrsDataSyncServiceOptions> optionsAccessor,
        ILogger<TrsDataSyncService> logger)
    {
        _crmEntityChangesService = crmEntityChangesService;
        _trsDataSyncHelper = trsDataSyncHelper;
        _logger = logger;
        _options = optionsAccessor.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        do
        {
            try
            {
                await ProcessChanges(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (ProcessChangesException ex)
            {
                _logger.LogError(ex.InnerException, ex.Message);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing entity changes.");
                return;
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task ProcessChanges(CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            _options.Entities,
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = _options.ProcessAllEntityTypesConcurrently ? MaxEntityTypesToProcessConcurrently : 1,
                CancellationToken = cancellationToken
            },
            async (entityType, ct) =>
            {
                try
                {
                    await ProcessChangesForEntityType(entityType, ct);
                }
                catch (Exception ex)
                {
                    throw new ProcessChangesException(entityType, ex);
                }
            });
    }

    internal async Task ProcessChangesForEntityType(string entityLogicalName, CancellationToken cancellationToken)
    {
        var totalProcessed = 0;

        var columns = new ColumnSet(_trsDataSyncHelper.GetSyncedAttributeNames(entityLogicalName));

        var modifiedSince = await _trsDataSyncHelper.GetLastModifiedOnForEntity(entityLogicalName);

        var changesEnumerable = _crmEntityChangesService.GetEntityChanges(ChangesKey, CrmClientName, entityLogicalName, columns, modifiedSince, PageSize)
            .WithCancellation(cancellationToken);

        await foreach (var changes in changesEnumerable)
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
            totalProcessed += newOrUpdatedItems.Count;

            // It's important deleted items are processed *after* upserts, otherwise we may resurrect a deleted record
            await HandleRemovedOrDeletedItems(removedOrDeletedItems, cancellationToken);
            totalProcessed += removedOrDeletedItems.Count;
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

        await _trsDataSyncHelper.SyncEntities(
            entityLogicalName,
            newOrUpdatedItems.Select(i => i.NewOrUpdatedEntity).ToArray(),
            _options.IgnoreInvalidData,
            cancellationToken);
    }

    private async Task HandleRemovedOrDeletedItems(
        IReadOnlyCollection<RemovedOrDeletedItem> removedOrDeletedItems,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (removedOrDeletedItems.Count == 0)
        {
            return;
        }

        var entityLogicalName = removedOrDeletedItems.First().RemovedItem.LogicalName;

        await _trsDataSyncHelper.DeleteEntities(entityLogicalName, removedOrDeletedItems.Select(i => i.RemovedItem.Id).ToArray(), cancellationToken);
    }
}

file class ProcessChangesException : Exception
{
    public ProcessChangesException(string entityType, Exception innerException)
        : base(GetMessage(entityType), innerException)
    {
        EntityType = entityType;
    }

    public string EntityType { get; }

    private static string GetMessage(string entityType) =>
        $"Failed processing changes for '{entityType}' entity.";
}
