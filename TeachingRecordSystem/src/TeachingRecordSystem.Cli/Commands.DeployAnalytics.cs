using Dfe.Analytics;
using Dfe.Analytics.EFCore;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateDeployAnalyticsCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };
        var airbyteConnectionIdOption = new Option<string>("--airbyte-connection-id") { Required = true };
        var airbyteClientIdOption = new Option<string>("--airbyte-client-id") { Required = true };
        var airbyteClientSecretOption = new Option<string>("--airbyte-client-secret") { Required = true };
        var airbyteApiBaseAddressOption = new Option<string>("--airbyte-api-base-address") { Required = true };
        var hiddenPolicyTagNameOption = new Option<string>("--hidden-policy-tag-name") { Required = true };
        var projectIdOption = new Option<string>("--project-id") { Required = true };
        var datasetIdOption = new Option<string>("--dataset-id") { Required = true };
        var googleCredentialsOption = new Option<string>("--google-credentials") { Required = true };

        if (configuration.GetConnectionString("DefaultConnection") is {} configuredConnectionString)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("deploy-analytics", "Deploys the DfE analytics configuration.")
        {
            connectionStringOption,
            airbyteConnectionIdOption,
            airbyteClientIdOption,
            airbyteClientSecretOption,
            airbyteApiBaseAddressOption,
            hiddenPolicyTagNameOption,
            projectIdOption,
            datasetIdOption,
            googleCredentialsOption
        };

        command.SetAction(
            async parseResult =>
            {
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);
                var airbyteConnectionId = parseResult.GetRequiredValue(airbyteConnectionIdOption);
                var hiddenPolicyTagName = parseResult.GetRequiredValue(hiddenPolicyTagNameOption);

                var services = new ServiceCollection()
                    .AddSingleton(configuration)
                    .AddDatabase(connectionString)
                    .AddDfeAnalytics(options =>
                    {
                        options.DatasetId = parseResult.GetRequiredValue(datasetIdOption);
                        options.ProjectId = parseResult.GetRequiredValue(projectIdOption);
                        options.CredentialsJson = parseResult.GetRequiredValue(googleCredentialsOption);
                    })
                    .AddDeploymentTools()
                    .ConfigureAirbyteApi(options =>
                    {
                        options.BaseAddress = parseResult.GetRequiredValue(airbyteApiBaseAddressOption);
                        options.ClientId = parseResult.GetRequiredValue(airbyteClientIdOption);
                        options.ClientSecret = parseResult.GetRequiredValue(airbyteClientSecretOption);
                    })
                    .Services
                    .BuildServiceProvider();

                var deployer = services.GetRequiredService<AnalyticsDeployer>();
                await using var dbContext = services.GetRequiredService<TrsDbContext>();

                await deployer.DeployAsync(dbContext, airbyteConnectionId, hiddenPolicyTagName);
            });

        return command;
    }
}
