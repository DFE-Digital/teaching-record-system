using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateSyncPersonCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };
        var crmConnectionStringOption = new Option<string>("--crm-connection-string") { IsRequired = true };
        var trnOption = new Option<string>("--trn") { IsRequired = true };

        PopulateOptionDefaultValueIfConfigured("ConnectionStrings:DefaultConnection", connectionStringOption);
        PopulateOptionDefaultValueIfConfigured("ConnectionStrings:Crm", crmConnectionStringOption);

        var command = new Command("sync-person", "Syncs a person from DQT to the TRS database.")
        {
            connectionStringOption,
            crmConnectionStringOption,
            trnOption
        };

        command.SetHandler(
            async (string connectionString, string crmConnectionString, string trn) =>
            {
                var serviceClient = new ServiceClient(crmConnectionString);

                var services = new ServiceCollection()
                    .AddCrmQueries()
                    .AddDefaultServiceClient(ServiceLifetime.Singleton, _ => serviceClient)
                    .AddNamedServiceClient(TrsDataSyncService.CrmClientName, ServiceLifetime.Singleton, _ => serviceClient)
                    .AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options, connectionString))
                    .AddTransient<TrsDataSyncHelper>()
                    .AddSingleton<IClock, Clock>()
                    .AddSingleton<ReferenceDataCache>()
                    .BuildServiceProvider();

                var crmQueryDispatcher = services.GetRequiredKeyedService<ICrmQueryDispatcher>(TrsDataSyncService.CrmClientName);
                var syncHelper = services.GetRequiredService<TrsDataSyncHelper>();

                var entityInfo = TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.Person);
                var contact = await crmQueryDispatcher.ExecuteQuery(new GetActiveContactByTrnQuery(trn, new Microsoft.Xrm.Sdk.Query.ColumnSet(entityInfo.AttributeNames)));

                if (contact is null)
                {
                    await Console.Error.WriteLineAsync($"Could not find contact with TRN: '{trn}'.");
                    // return 1;  // TODO waiting on a release with https://github.com/dotnet/command-line-api/pull/2092
                    return;
                }

                await syncHelper.SyncPerson(contact, ignoreInvalid: false);
                //return 0;
            },
            connectionStringOption,
            crmConnectionStringOption,
            trnOption);

        return command;

        void PopulateOptionDefaultValueIfConfigured(string configurationKey, Option option)
        {
            var configuredValue = configuration[configurationKey];

            if (configuredValue is not null)
            {
                option.SetDefaultValue(configuredValue);
            }
        }
    }
}
