using Dfe.Analytics.EFCore.AirbyteApi;
using Dfe.Analytics.EFCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace Dfe.Analytics.EFCore;

public class AnalyticsDeployer(ConfigurationProvider configurationProvider, AirbyteApiClient airbyteApiClient)
{
    public async Task DeployAsync(DbContext dbContext, string airbyteConnectionId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(airbyteConnectionId);

        if (dbContext.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException("Cannot deploy analytics configuration within an active database transaction.");
        }

        var configuration = configurationProvider.GetConfiguration(dbContext);
        var deploymentHelper = new AirbyteDeploymentHelper(configuration);

        // TODO optimize to avoid unnecessary updates

        // JsonElement? currentStreamConfiguration;  // TODO
        // JsonElement newStreamConfiguration = deploymentHelper.CreateStreamsConfiguration(configuration);
        // var airbyteConfigurationHasChanged = !JsonElement.DeepEquals(currentStreamConfiguration, newStreamConfiguration);
        //
        // if (!airbyteConfigurationHasChanged)
        // {
        //     return;
        // }

        await deploymentHelper.ConfigurePublicationAsync(
            (NpgsqlConnection)dbContext.Database.GetDbConnection(),
            cancellationToken);

        await deploymentHelper.SetAirbyteConfigurationAsync(
            airbyteConnectionId,
            airbyteApiClient,
            cancellationToken);

        // TODO BQ tags
    }
}
