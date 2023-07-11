using Microsoft.Xrm.Sdk.Metadata;
using TeachingRecordSystem.Dqt;
using TeachingRecordSystem.Dqt.Services.DqtReporting;

namespace TeachingRecordSystem.Cli;

public partial class Commands
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
                var entityMetadata = await DataverseAdapter.GetEntityMetadata(serviceClient, entityType, EntityFilters.Default | EntityFilters.Attributes);
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
