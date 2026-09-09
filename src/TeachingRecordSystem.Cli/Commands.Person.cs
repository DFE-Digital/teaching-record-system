using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreatePersonCommand(IConfiguration configuration)
    {
        return new Command("person", "Commands for managing person records.")
        {
            CreateUnmergeCommand()
        };

        Command CreateUnmergeCommand()
        {
            var trnOption = new Option<string>("--trn") { Required = true };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("unmerge", "Un-merges a record that was deactivated by a merge from the record it was merged into.")
            {
                trnOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var trn = parseResult.GetRequiredValue(trnOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    var services = new ServiceCollection()
                        .AddTimeProvider()
                        .AddDatabase(connectionString)
                        .AddMemoryCache()
                        .AddWebhookMessageFactory()
                        .AddEventPublisher()
                        .BuildServiceProvider();

                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                    var person = await dbContext.Persons
                        .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                        .SingleOrDefaultAsync(p => p.Trn == trn);

                    if (person is null)
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine("Person was not found");
                        return 1;
                    }

                    if (person.MergedWithPersonId is not Guid mergedWithPersonId)
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine("Person has not been merged with another record");
                        return 1;
                    }

                    var mergedWithPersonTrn = await dbContext.Persons
                        .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                        .Where(p => p.PersonId == mergedWithPersonId)
                        .Select(p => p.Trn)
                        .SingleAsync();

                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                    var now = timeProvider.UtcNow;

                    var processContext = new ProcessContext(ProcessType.PersonUnmerging, now, SystemUser.SystemUserId);

                    // The event scope runs its handlers when it's disposed, so it has to close before the transaction
                    // is completed.
                    await using (var eventScope = eventPublisher.GetOrCreateEventScope(processContext))
                    {
                        // Only the deactivation and the link back to the retained record are reversed; the details the
                        // merge copied onto the retained record and any One Login users it re-pointed are left alone.
                        person.Status = PersonStatus.Active;
                        person.MergedWithPersonId = null;
                        person.UpdatedOn = now;

                        await dbContext.SaveChangesAsync();

                        await eventScope.PublishEventAsync(
                            new PersonUnmergedEvent
                            {
                                EventId = Guid.NewGuid(),
                                PersonId = person.PersonId,
                                UnmergedFromPersonId = mergedWithPersonId
                            });
                    }

                    transaction.Complete();

                    parseResult.InvocationConfiguration.Output.WriteLine(
                        $"Un-merged TRN {trn} from TRN {mergedWithPersonTrn}.");
                    parseResult.InvocationConfiguration.Output.WriteLine(
                        $"WARNING: any One Login users that were linked to TRN {trn} before the merge remain linked to TRN {mergedWithPersonTrn} and have not been moved back.");
                    return 0;
                });

            return command;
        }
    }
}
