using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.Services.CrmEntityChanges;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public class TrsDataSyncService(
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] ICrmEntityChangesService crmEntityChangesService,
    TrsDataSyncHelper trsDataSyncHelper,
    IOptions<TrsDataSyncServiceOptions> optionsAccessor,
    ILogger<TrsDataSyncService> logger) : BackgroundService
{
    public const string CrmClientName = "TrsDataSync";
    private const string ChangesKeyPrefix = "TrsDataSync";

    private static readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions()
        {
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = 10
        })
        .Build();

    private const int PageSize = 1000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(optionsAccessor.Value.PollIntervalSeconds));

        do
        {
            try
            {
                await _resiliencePipeline.ExecuteAsync(async ct => await ProcessChanges(ct), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (ProcessChangesException ex)
            {
                logger.LogError(ex.InnerException, ex.Message);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing entity changes.");
                return;
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task ProcessChanges(CancellationToken cancellationToken)
    {
        var modelTypesToSync = optionsAccessor.Value.ModelTypes;

        // Order is important here; the dependees should come before dependents
        await SyncIfEnabled(TrsDataSyncHelper.ModelTypes.Person);
        await SyncIfEnabled(TrsDataSyncHelper.ModelTypes.MandatoryQualification);

        async Task SyncIfEnabled(string modelType)
        {
            if (modelTypesToSync.Contains(modelType))
            {
                try
                {
                    await ProcessChangesForModelType(modelType, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ProcessChangesException(modelType, ex);
                }
            }
        }
    }

    internal async Task ProcessChangesForModelType(string modelType, CancellationToken cancellationToken)
    {
        // There are few CRM entity types that map to multiple model types (e.g. Qualification) -
        // we want to sync them separately.
        // We do this by having a separate `changesKey` for each model type.

        var changesKey = modelType switch
        {
            TrsDataSyncHelper.ModelTypes.Person => ChangesKeyPrefix,  // Earlier version used "TrsDataSync" alone for syncing contacts - maintain that
            _ => $"{ChangesKeyPrefix}:{modelType}"
        };

        var (entityLogicalName, attributeNames) = TrsDataSyncHelper.GetEntityInfoForModelType(modelType);
        var columns = new ColumnSet(attributeNames);

        var modifiedSince = await trsDataSyncHelper.GetLastModifiedOnForModelType(modelType);

        var changesEnumerable = crmEntityChangesService.GetEntityChanges(changesKey, entityLogicalName, columns, modifiedSince, PageSize)
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

            await trsDataSyncHelper.SyncRecords(
                modelType,
                newOrUpdatedItems.Select(i => i.NewOrUpdatedEntity).ToArray(),
                optionsAccessor.Value.IgnoreInvalidData,
                cancellationToken);

            await trsDataSyncHelper.DeleteRecords(
                modelType,
                removedOrDeletedItems.Select(i => i.RemovedItem.Id).ToArray(),
                cancellationToken);
        }
    }
}

file class ProcessChangesException : Exception
{
    public ProcessChangesException(string modelType, Exception innerException)
        : base(GetMessage(modelType), innerException)
    {
        ModelType = modelType;
    }

    public string ModelType { get; }

    private static string GetMessage(string modelType) =>
        $"Failed processing changes for '{modelType}' data.";
}
