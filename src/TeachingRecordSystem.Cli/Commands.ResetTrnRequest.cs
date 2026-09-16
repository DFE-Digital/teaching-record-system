using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
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
                    .AddTimeProvider()
                    .AddLogging()
                    .AddDatabase(connectionString)
                    .AddMemoryCache()
                    .AddWebhookMessageFactory()
                    .AddEventPublisher()
                    .AddPersonService()
                    .AddOneLoginService()
                    .AddSupportTaskServices()
                    .AddTrnRequestService(configuration)
                    // TrnRequestService's dependencies pull in a job scheduler that resetting never uses;
                    // this one fails loudly rather than letting the command queue any work.
                    .AddSingleton<IBackgroundJobScheduler, UnavailableBackgroundJobScheduler>()
                    .BuildServiceProvider();

                using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                using var scope = services.CreateScope();
                var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

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

                var now = timeProvider.UtcNow;

                // Matching is re-run for its side effect on the request's PotentialDuplicate flag, which is saved
                // below. The outcome itself isn't needed: the support task doesn't store the matches, and the
                // resolve journey matches again when it's opened, handling all three outcomes.
                await trnRequestService.MatchPersonsAsync(request);

                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var processContext = new ProcessContext(ProcessType.TrnRequestResetting, now, SystemUser.SystemUserId);

                SupportTask supportTask;

                // The event scope runs its handlers when it's disposed, so it has to close before the transaction
                // is completed - nothing can touch the database once that's done.
                await using (var eventScope = eventPublisher.GetOrCreateEventScope(processContext))
                {
                    var oldTrnRequest = EventModels.TrnRequestMetadata.FromModel(request);
                    request.ResolvedPersonId = null;
                    request.Status = TrnRequestStatus.Pending;

                    var changes = (oldTrnRequest.Status != request.Status ? TrnRequestUpdatedChanges.Status : 0) |
                        (oldTrnRequest.ResolvedPersonId != request.ResolvedPersonId ? TrnRequestUpdatedChanges.ResolvedPersonId : 0);

                    var subject = SupportTask.Subject.FromTrnRequest(request);
                    supportTask = new SupportTask
                    {
                        CreatedOn = now,
                        UpdatedOn = now,
                        SupportTaskType = SupportTaskType.TrnRequest,
                        OneLoginUserSubject = request.OneLoginUserSubject,
                        TrnRequestApplicationUserId = request.ApplicationUserId,
                        TrnRequestId = request.RequestId,
                        SubjectName = subject.Name,
                        SubjectEmailAddress = subject.EmailAddress,
                        SourceApplicationUserId = request.ApplicationUserId,
                        Data = new TrnRequestData()
                    };

                    dbContext.SupportTasks.Add(supportTask);
                    await dbContext.SaveChangesAsync();

                    await eventScope.PublishEventAsync(
                        new SupportTaskCreatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            SupportTask = EventModels.SupportTask.FromModel(supportTask)
                        });

                    await eventScope.PublishEventAsync(
                        new TrnRequestUpdatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            SourceApplicationUserId = sourceApplicationUserId,
                            RequestId = trnRequestId,
                            Changes = changes,
                            TrnRequest = EventModels.TrnRequestMetadata.FromModel(request),
                            OldTrnRequest = oldTrnRequest,
                            ReasonDetails = reason
                        });
                }

                transaction.Complete();

                parseResult.InvocationConfiguration.Output.WriteLine(
                    $"Created new {supportTask.SupportTaskType} support task: {supportTask.SupportTaskReference}");
                return 0;
            });

        return command;
    }
}
