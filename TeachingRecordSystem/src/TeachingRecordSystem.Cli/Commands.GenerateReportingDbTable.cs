using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk.Metadata;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateGenerateReportingDbTableCommand(IConfiguration configuration)
    {
        var crmConnectionStringOption = new Option<string>("--crm-connection-string") { IsRequired = true };
        var entityTypeOption = new Option<string>("--entity-type") { IsRequired = true };

        PopulateOptionDefaultValueIfConfigured("ConnectionStrings:Crm", crmConnectionStringOption);

        var command = new Command("generate-reporting-db-table", "Returns a T-SQL create statement for the specified entity type.")
        {
            crmConnectionStringOption,
            entityTypeOption
        };

        command.SetHandler(
            async (string crmConnectionString, string entityType) =>
            {
                var serviceClient = new ServiceClient(crmConnectionString);

                var services = new ServiceCollection()
                    .AddCrmQueries()
                    .AddDefaultServiceClient(ServiceLifetime.Singleton, _ => serviceClient)
                    .BuildServiceProvider();

                var crmQueryDispatcher = services.GetRequiredService<ICrmQueryDispatcher>();

                var entityMetadata = await crmQueryDispatcher.ExecuteQuery(
                    new GetEntityMetadataQuery(entityType, EntityFilters.Default | EntityFilters.Attributes));
                var entityTableMapping = EntityTableMapping.Create(entityMetadata);
                var sql = entityTableMapping.GetCreateTableSql();
                await Console.Out.WriteLineAsync(sql);
            },
            crmConnectionStringOption,
            entityTypeOption);

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
