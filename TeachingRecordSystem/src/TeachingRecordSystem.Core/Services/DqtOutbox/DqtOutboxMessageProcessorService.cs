using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.CrmEntityChanges;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Services.DqtOutbox;

public class DqtOutboxMessageProcessorService(
    [FromKeyedServices(DqtOutboxMessageProcessorService.CrmClientName)] ICrmEntityChangesService crmEntityChangesService,
    OutboxMessageHandler outboxMessageHandler,
    IDbContextFactory<TrsDbContext> dbContextFactory)
    : BackgroundService
{
    private const string CrmClientName = TrsDataSyncService.CrmClientName;
    private const string ChangesKey = "ProcessDqtOutboxMessagesService";
    private const string EntityName = dfeta_TrsOutboxMessage.EntityLogicalName;
    private const int PageSize = 50;
    private const string DateTimeMetadataFormat = "o";

    private static readonly ColumnSet _columns = new(
        "createdon",
        dfeta_TrsOutboxMessage.Fields.dfeta_MessageName,
        dfeta_TrsOutboxMessage.Fields.dfeta_Payload);

    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(60);

    private static readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions()
        {
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = 10
        })
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateTime? ignoreUntil = null;

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var ignoreUntilMetadata =
                await dbContext.OutboxMessageProcessorMetadata.SingleOrDefaultAsync(o =>
                    o.Key == OutboxMessageProcessorMetadata.Keys.IgnoreUntil);

            if (ignoreUntilMetadata is not null)
            {
                ignoreUntil = DateTime.ParseExact(ignoreUntilMetadata.Value, DateTimeMetadataFormat, provider: null);
            }
        }

        using var timer = new PeriodicTimer(_pollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                await foreach (var changedItems in crmEntityChangesService.GetEntityChangesAsync(
                    ChangesKey,
                    EntityName,
                    _columns,
                    modifiedSince: null,
                    PageSize).WithCancellation(ct))
                {
                    foreach (var changedItem in changedItems)
                    {
                        if (changedItem is not NewOrUpdatedItem newOrUpdatedItem)
                        {
                            throw new NotSupportedException(
                                $"Unsupported {nameof(IChangedItem)}: '{changedItem.GetType().Name}'.");
                        }

                        var message = newOrUpdatedItem.NewOrUpdatedEntity.ToEntity<dfeta_TrsOutboxMessage>();
                        var createdOn = message.GetAttributeValue<DateTime>("createdon");

                        if (createdOn <= ignoreUntil)
                        {
                            continue;
                        }

                        using var txn = new TransactionScope(TransactionScopeOption.RequiresNew,
                            TransactionScopeAsyncFlowOption.Enabled);

                        await outboxMessageHandler.HandleOutboxMessageAsync(message);

                        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
                        {
                            var ignoreUntilMetadata =
                                await dbContext.OutboxMessageProcessorMetadata.SingleOrDefaultAsync(o =>
                                    o.Key == OutboxMessageProcessorMetadata.Keys.IgnoreUntil);

                            if (ignoreUntilMetadata is null)
                            {
                                dbContext.OutboxMessageProcessorMetadata.Add(new OutboxMessageProcessorMetadata()
                                {
                                    Key = OutboxMessageProcessorMetadata.Keys.IgnoreUntil,
                                    Value = createdOn.ToString(DateTimeMetadataFormat)
                                });
                            }
                            else
                            {
                                ignoreUntilMetadata.Value = createdOn.ToString(DateTimeMetadataFormat);
                            }

                            await dbContext.SaveChangesAsync();
                        }

                        txn.Complete();
                    }
                }
            },
            stoppingToken);
        }
    }
}
