using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateDeleteSupportTaskCommand(IConfiguration configuration)
    {
        var supportTaskReferenceOption = new Option<string>("--support-task-reference", "--ref") { Required = true };
        var reasonOption = new Option<string>("--reason") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("delete-support-task", "Deletes a support task.")
        {
            supportTaskReferenceOption,
            reasonOption,
            connectionStringOption
        };

        command.SetAction(
            async parseResult =>
            {
                var supportTaskReference = parseResult.GetRequiredValue(supportTaskReferenceOption);
                var reason = parseResult.GetRequiredValue(reasonOption);
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                var services = new ServiceCollection()
                    .AddClock()
                    .AddDatabase(connectionString)
                    .AddEventPublisher()
                    .AddSupportTaskService()
                    .BuildServiceProvider();

                using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                using var scope = services.CreateScope();
                var supportTaskService = scope.ServiceProvider.GetRequiredService<SupportTaskService>();
                var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                var processContext = new ProcessContext(ProcessType.SupportTaskDeleting, timeProvider.UtcNow, SystemUser.SystemUserId);

                try
                {
                    await supportTaskService.DeleteSupportTaskAsync(new DeleteSupportTaskOptions(supportTaskReference, reason), processContext);
                }
                catch (NotFoundException ex) when (ex.EntityName == nameof(SupportTask))
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("Support task was not found");
                    return 1;
                }

                transaction.Complete();

                return 0;
            });

        return command;
    }
}
