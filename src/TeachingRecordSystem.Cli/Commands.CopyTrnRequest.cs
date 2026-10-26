using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    // Teacher Auth used to hand a client the ID of a dormant TRN request that another application user owned, which
    // the API, scoped by application user, then reports as not existing. This creates a dormant copy of the request
    // under the same ID for the client that was given it, so the ID it already holds starts working.
    public static Command CreateCopyTrnRequestCommand(IConfiguration configuration)
    {
        var trnRequestIdOption = new Option<string>("--trn-request-id", "--id") { Required = true };
        var sourceApplicationUserIdOption = new Option<Guid>("--source-application-user-id") { Required = true };
        var targetApplicationUserIdOption = new Option<Guid>("--target-application-user-id") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("copy-trn-request", "Copies a Teacher Auth TRN request to another application user as a dormant request with the same ID.")
        {
            trnRequestIdOption,
            sourceApplicationUserIdOption,
            targetApplicationUserIdOption,
            connectionStringOption
        };

        command.SetAction(
            async parseResult =>
            {
                var trnRequestId = parseResult.GetRequiredValue(trnRequestIdOption);
                var sourceApplicationUserId = parseResult.GetRequiredValue(sourceApplicationUserIdOption);
                var targetApplicationUserId = parseResult.GetRequiredValue(targetApplicationUserIdOption);
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                var services = new ServiceCollection()
                    .AddTimeProvider()
                    .AddLogging()
                    .AddDatabase(connectionString)
                    .AddMemoryCache()
                    .AddWebhookMessageFactory()
                    .AddEventPublisher()
                    .AddPersonService()
                    .AddRoutesToProfessionalStatusService()
                    .AddSingleton<ReferenceDataCache>()
                    .AddOneLoginService()
                    .AddSupportTaskServices()
                    .AddTrnRequestService(configuration)
                    // A dormant request is never resolved on creation, so nothing should be queued.
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

                // The copy relies on the verified One Login user to resolve to the same person as the original
                if (request.OneLoginUserSubject is null || request.IdentityVerified is not true)
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("TRN request does not have a verified One Login user");
                    return 1;
                }

                if (!await dbContext.ApplicationUsers.AnyAsync(u => u.UserId == targetApplicationUserId))
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("Target application user was not found");
                    return 1;
                }

                if (await dbContext.TrnRequestMetadata.AnyAsync(r => r.ApplicationUserId == targetApplicationUserId && r.RequestId == trnRequestId))
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("Target application user already has a TRN request with this ID");
                    return 1;
                }

                var trnRequestService = scope.ServiceProvider.GetRequiredService<TrnRequestService>();

                var processContext = new ProcessContext(ProcessType.TrnRequestCreating, timeProvider.UtcNow, SystemUser.SystemUserId);

                await trnRequestService.CreateTrnRequestAsync(
                    new CreateTrnRequestOptions
                    {
                        TryResolve = false,
                        ApplicationUserId = targetApplicationUserId,
                        RequestId = trnRequestId,
                        OneLoginUserInfo = new CreateTrnRequestOptionsOneLoginUserInfo(request.OneLoginUserSubject, IdentityVerified: true),
                        EmailAddress = request.EmailAddress,
                        FirstName = request.FirstName,
                        MiddleName = request.MiddleName,
                        LastName = request.LastName,
                        DateOfBirth = request.DateOfBirth,
                        NationalInsuranceNumber = request.NationalInsuranceNumber,
                        Gender = request.Gender
                    },
                    processContext);

                transaction.Complete();

                parseResult.InvocationConfiguration.Output.WriteLine(
                    $"Created dormant TRN request {trnRequestId} for application user {targetApplicationUserId}");
                return 0;
            });

        return command;
    }
}
