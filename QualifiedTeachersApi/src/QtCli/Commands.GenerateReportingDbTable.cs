using Microsoft.Xrm.Sdk.Metadata;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.DqtReporting;

namespace QtCli;

public partial class Commands
{
    public static Command CreateGenerateReportingDbTableCommand(IConfiguration configuration)
    {
        var crmUrlOption = new Option<string>("--crm-url") { IsRequired = true };
        var crmClientIdOption = new Option<string>("--crm-client-id") { IsRequired = true };
        var crmClientSecretOption = new Option<string>("--crm-client-secret") { IsRequired = true };
        var entityTypeOption = new Option<string>("--entity-type") { IsRequired = true };

        PopulateOptionDefaultValueIfConfigured("CrmUrl", crmUrlOption);
        PopulateOptionDefaultValueIfConfigured("CrmClientId", crmClientIdOption);
        PopulateOptionDefaultValueIfConfigured("CrmClientSecret", crmClientSecretOption);

        var command = new Command("generate-reporting-db-table", "Returns a T-SQL create statement for the specified entity type.")
        {
            crmUrlOption,
            crmClientIdOption,
            crmClientSecretOption,
            entityTypeOption
        };

        command.SetHandler(
            async (string crmUrl, string crmClientId, string crmClientSecret, string entityType) =>
            {
                var serviceClient = new ServiceClient(new Uri(crmUrl), crmClientId, crmClientSecret, useUniqueInstance: true);
                var entityMetadata = await DataverseAdapter.GetEntityMetadata(serviceClient, entityType, EntityFilters.Default | EntityFilters.Attributes);
                var entityTableMapping = EntityTableMapping.Create(entityMetadata);
                var sql = entityTableMapping.GetCreateTableSql();
                await Console.Out.WriteLineAsync(sql);
            },
            crmUrlOption,
            crmClientIdOption,
            crmClientSecretOption,
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
