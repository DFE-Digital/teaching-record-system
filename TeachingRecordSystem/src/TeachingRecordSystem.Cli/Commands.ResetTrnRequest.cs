using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.Something;

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
                    .AddPersonMatching()
                    .AddSomething()
                    .BuildServiceProvider();

                using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                using var scope = services.CreateScope();
                var clock = scope.ServiceProvider.GetRequiredService<IClock>();

                var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();

                var somethingService = scope.ServiceProvider.GetRequiredService<SomethingService>();

                var request = new ResetTrnRequestInfo
                {
                    ApplicationUserId = sourceApplicationUserId,
                    RequestId = trnRequestId,
                    Reason = reason
                };

                try
                {
                    var result = await somethingService.ResetTrnRequestAsync(request);
                    transaction.Complete();

                    parseResult.InvocationConfiguration.Output.WriteLine(
                        $"Created new {result.SupportTaskType} support task: {result.SupportTaskReference}");
                    return 0;
                }
                catch (TrnRequestNotFoundException)
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("TRN request was not found");
                    return 1;
                }
                catch (TrnRequestAlreadyHasOpenSupportTasksException)
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("TRN request already has open support tasks");
                    return 1;
                }
            });

        return command;
    }
}
