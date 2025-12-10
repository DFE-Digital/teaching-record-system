using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateResetTrnRequestCommand(IConfiguration configuration)
    {
        var trnRequestIdOption = new Option<string>("--trn-request-id", "--id") { Required = true };
        var sourceApplicationUserIdOption = new Option<Guid>("--source-application-user-id") { Required = true };
        var reasonOption = new Option<string>("--reason") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("reset-trn-request", "Resets a TRN request.")
        {
            trnRequestIdOption,
            sourceApplicationUserIdOption,
            reasonOption,
            connectionStringOption
        };

        command.SetAction(
            async parseResult =>
            {
                var trnRequestId = parseResult.GetRequiredValue(trnRequestIdOption);
                var sourceApplicationUserId = parseResult.GetRequiredValue(sourceApplicationUserIdOption);
                var reason = parseResult.GetRequiredValue(reasonOption);
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                var services = new ServiceCollection()
                    .AddClock()
                    .AddDatabase(connectionString)
                    .AddEventPublisher()
                    .BuildServiceProvider();

                using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                using var scope = services.CreateScope();
                var clock = scope.ServiceProvider.GetRequiredService<IClock>();

                var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();

                var request = await dbContext.TrnRequestMetadata
                    .SingleOrDefaultAsync(r => r.ApplicationUserId == sourceApplicationUserId && r.RequestId == trnRequestId);

                if (request is null)
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("TRN request was not found");
                    return 1;
                }

                // Check if there are any Open support tasks already for this request - we can't reset if there are
                var haveSupportTasksForRequest = await dbContext.SupportTasks
                    .Where(t => t.TrnRequestApplicationUserId == sourceApplicationUserId && t.TrnRequestId == trnRequestId)
                    .Where(t => t.Status == SupportTaskStatus.Open)
                    .AnyAsync();

                if (haveSupportTasksForRequest)
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("TRN request already has open support tasks");
                    return 1;
                }

                var trnRequestService = scope.ServiceProvider.GetRequiredService<TrnRequestService>();

                var now = clock.UtcNow;

                var matchResult = await trnRequestService.MatchPersonsAsync(request);

                // We only handle PotentialMatches for now
                if (matchResult.Outcome is not MatchPersonsResultOutcome.PotentialMatches)
                {
                    throw new NotImplementedException();
                }

                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var processContext = new ProcessContext(ProcessType.TrnRequestResetting, now, SystemUser.SystemUserId);

                var oldTrnRequest = EventModels.TrnRequestMetadata.FromModel(request);
                request.Reset();

                var changes = (oldTrnRequest.Status != request.Status ? TrnRequestUpdatedChanges.Status : 0) |
                    (oldTrnRequest.ResolvedPersonId != request.ResolvedPersonId ? TrnRequestUpdatedChanges.ResolvedPersonId : 0);

                var supportTask = SupportTask.Create(
                    SupportTaskType.ApiTrnRequest,
                    new ApiTrnRequestData(),
                    personId: null,
                    request.OneLoginUserSubject,
                    request.ApplicationUserId,
                    request.RequestId,
                    createdBy: SystemUser.SystemUserId,
                    now,
                    out _);

                dbContext.SupportTasks.Add(supportTask);
                await dbContext.SaveChangesAsync();

                await eventPublisher.PublishEventAsync(
                    new SupportTaskCreatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        SupportTask = EventModels.SupportTask.FromModel(supportTask)
                    },
                    processContext);

                await eventPublisher.PublishEventAsync(
                    new TrnRequestUpdatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        SourceApplicationUserId = sourceApplicationUserId,
                        RequestId = trnRequestId,
                        Changes = changes,
                        TrnRequest = EventModels.TrnRequestMetadata.FromModel(request),
                        OldTrnRequest = oldTrnRequest,
                        ReasonDetails = reason
                    },
                    processContext);

                transaction.Complete();

                parseResult.InvocationConfiguration.Output.WriteLine(
                    $"Created new {supportTask.SupportTaskType} support task: {supportTask.SupportTaskReference}");
                return 0;
            });

        return command;
    }
}
