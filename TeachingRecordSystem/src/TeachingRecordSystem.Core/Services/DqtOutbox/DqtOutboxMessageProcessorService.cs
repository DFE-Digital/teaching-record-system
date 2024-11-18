using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.Services.CrmEntityChanges;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Services.DqtOutbox;

public class DqtOutboxMessageProcessorService(
    [FromKeyedServices(DqtOutboxMessageProcessorService.CrmClientName)] ICrmEntityChangesService crmEntityChangesService,
    OutboxMessageHandler outboxMessageHandler)
    : BackgroundService
{
    private const string CrmClientName = TrsDataSyncService.CrmClientName;
    private const string ChangesKey = "ProcessDqtOutboxMessagesService";
    private const string EntityName = dfeta_TrsOutboxMessage.EntityLogicalName;
    private const int PageSize = 50;

    private static readonly ColumnSet _columns = new(
        dfeta_TrsOutboxMessage.Fields.dfeta_MessageName,
        dfeta_TrsOutboxMessage.Fields.dfeta_Payload);

    private static readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions()
        {
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = 10
        })
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            await foreach (var changedItems in crmEntityChangesService.GetEntityChangesAsync(
                ChangesKey,
                EntityName,
                _columns,
                modifiedSince: null,
                PageSize,
                cancellationToken: ct))
            {
                foreach (var changedItem in changedItems)
                {
                    if (changedItem is not NewOrUpdatedItem newOrUpdatedItem)
                    {
                        throw new NotSupportedException($"Unsupported {nameof(IChangedItem)}: '{changedItem.GetType().Name}'.");
                    }

                    var message = newOrUpdatedItem.NewOrUpdatedEntity.ToEntity<dfeta_TrsOutboxMessage>();
                    await outboxMessageHandler.HandleOutboxMessageAsync(message);
                }
            }
        },
        stoppingToken);
}
