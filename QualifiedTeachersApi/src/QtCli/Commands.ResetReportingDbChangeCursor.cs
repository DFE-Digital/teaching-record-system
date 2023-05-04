using Medallion.Threading.FileSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk.Query;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Services.CrmEntityChanges;
using QualifiedTeachersApi.Services.DqtReporting;

namespace QtCli;

public static partial class Commands
{
    public static Command CreateResetReportingDbChangeCursorCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };
        var crmUrlOption = new Option<string>("--crm-url") { IsRequired = true };
        var crmClientIdOption = new Option<string>("--crm-client-id") { IsRequired = true };
        var crmClientSecretOption = new Option<string>("--crm-client-secret") { IsRequired = true };
        var entityTypesOption = new Option<IEnumerable<string>>("--entity-types") { IsRequired = true, AllowMultipleArgumentsPerToken = true };

        PopulateOptionDefaultValueIfConfigured("ConnectionStrings:DefaultConnection", connectionStringOption);
        PopulateOptionDefaultValueIfConfigured("CrmUrl", crmUrlOption);
        PopulateOptionDefaultValueIfConfigured("CrmClientId", crmClientIdOption);
        PopulateOptionDefaultValueIfConfigured("CrmClientSecret", crmClientSecretOption);

        var command = new Command("reset-reporting-db-change-cursor", "Resets the entity changes cursor to the current change token for the DqtReporting service.")
        {
            connectionStringOption,
            crmUrlOption,
            crmClientIdOption,
            crmClientSecretOption,
            entityTypesOption
        };

        command.SetHandler(
            async (string connectionString, string crmUrl, string crmClientId, string crmClientSecret, IEnumerable<string> entityTypes) =>
            {
                var serviceProvider = new ServiceCollection()
                    .AddDbContextFactory<DqtContext>(options => DqtContext.ConfigureOptions(options, connectionString))
                    .BuildServiceProvider();

                var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<DqtContext>>();

                var serviceClient = new ServiceClient(new Uri(crmUrl), crmClientId, crmClientSecret, useUniqueInstance: true);

                // We assume that no other processing is going on while this command is running so a local lock is ok
                var lockFileDirectory = Path.Combine(Path.GetTempPath(), "qtlocks");
                var distributedLockProvider = new FileDistributedSynchronizationProvider(new DirectoryInfo(lockFileDirectory));

                var entityChangesService = new CrmEntityChangesService(dbContextFactory, serviceClient, distributedLockProvider);
                var emptyColumnSet = new ColumnSet();

                await Task.WhenAll(entityTypes.Select(ProcessChangesForEntityType));

                async Task ProcessChangesForEntityType(string entityType)
                {
                    await foreach (var changes in entityChangesService.GetEntityChanges(DqtReportingService.ChangesKey, entityType, emptyColumnSet, pageSize: 10000))
                    {
                    }
                }
            },
            connectionStringOption,
            crmUrlOption,
            crmClientIdOption,
            crmClientSecretOption,
            entityTypesOption);

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
