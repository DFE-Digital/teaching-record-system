using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.Analytics.EFCore.AirbyteApi;
using Dfe.Analytics.EFCore.Configuration;
using Npgsql;

namespace Dfe.Analytics.EFCore;

public class AirbyteDeploymentHelper(DatabaseSyncConfiguration configuration)
{
    private const string PublicationName = "airbyte_publication";

    public async Task ConfigurePublicationAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var syncedTableNames = configuration.Tables.Select(t => t.Name).ToArray();
        if (syncedTableNames.Length == 0)
        {
            throw new InvalidOperationException("No tables are configured for synchronization.");
        }

        var updatePublicationSql =
            $"""
             ALTER PUBLICATION {PublicationName}
             SET TABLE {string.Join(", ", syncedTableNames.Select(n => $"\"{n}\""))};
             """;

        var createPublicationSql =
            $"""
             CREATE PUBLICATION {PublicationName}
             FOR TABLE {string.Join(", ", syncedTableNames.Select(n => $"\"{n}\""))};
             """;

        var dropPublicationSql = $"DROP PUBLICATION {PublicationName};";

        var startedOpen = connection.State is ConnectionState.Open;
        if (!startedOpen)
        {
            Debug.Assert(connection.State is ConnectionState.Closed);
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await ExecuteSqlAsync(updatePublicationSql);
        }
        catch (PostgresException ex) when (ex.SqlState is PostgresErrorCodes.ObjectNotInPrerequisiteState)
        {
            // Publication exists, but it's defined 'FOR ALL TABLES', so we need to drop and recreate it
            await ExecuteSqlAsync(dropPublicationSql);
            await ExecuteSqlAsync(createPublicationSql);
        }
        catch (PostgresException ex) when (ex.SqlState is PostgresErrorCodes.UndefinedObject)
        {
            // Publication does not exist, create it
            await ExecuteSqlAsync(createPublicationSql);
        }
        finally
        {
            if (!startedOpen)
            {
                await connection.CloseAsync();
            }
        }

        async Task ExecuteSqlAsync(string commandText)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task SetAirbyteConfigurationAsync(
        string connectionId,
        AirbyteApiClient airbyteApiClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionId);

        var streamsConfig = GetUpdateConnectionDetailsRequestFromConfiguration();
        await airbyteApiClient.UpdateConnectionDetailsAsync(connectionId, streamsConfig, cancellationToken);
    }

    // public async Task WaitForJobToCompleteAsync(long jobId, CancellationToken cancellationToken)
    // {
    //     using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
    //
    //     while (await timer.WaitForNextTickAsync(cancellationToken))
    //     {
    //         var jobStatusResponse = await airbyteApiClient.GetJobStatusAsync(jobId, cancellationToken);
    //
    //         if (jobStatusResponse.Status is JobStatuses.Succeeded)
    //         {
    //             return;
    //         }
    //
    //         if (jobStatusResponse.Status is JobStatuses.Failed or JobStatuses.Cancelled)
    //         {
    //             throw new InvalidOperationException($"Airbyte job {jobId} failed with status '{jobStatusResponse.Status}'.");
    //         }
    //     }
    // }

    private UpdateConnectionDetailsRequest GetUpdateConnectionDetailsRequestFromConfiguration()
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new UpdateConnectionDetailsRequest
        {
            Configurations =
            [
                new UpdateConnectionDetailsRequestConfiguration
                {
                    Streams = configuration.Tables.Select(t => new UpdateConnectionDetailsRequestConfigurationStream
                    {
                        Name = t.Name,
                        SyncMode = "incremental_append",
                        CursorField = ["_ab_cdc_lsn"],
                        PrimaryKey = [t.PrimaryKey.ColumnNames],
                        SelectedFields = new[] { "_ab_cdc_lsn", "_ab_cdc_deleted_at", "_ab_cdc_updated_at" }.Concat(t.Columns.Select(c => c.Name))
                            .Select(n => new UpdateConnectionDetailsRequestConfigurationStreamField
                            {
                                FieldPath = [n]
                            })
                    })
                }
            ]
        };
    }

    // public async Task<JsonElement> GetStreamsConfigurationAsync(string connectionId, CancellationToken cancellationToken)
    // {
    //     var connectionResponse = await airbyteApiClient.GetConnectionAsync(connectionId, cancellationToken);
    //     return connectionResponse.Configuration;
    // }

    // private async Task<GetJobsListResponseDataItem?> GetLastJobAsync(string connectionId, CancellationToken cancellationToken)
    // {
    //     var jobsList = await airbyteApiClient.GetJobsListAsync(
    //         connectionId: connectionId,
    //         jobType: JobTypes.Sync,
    //         limit: 1,
    //         orderBy: "createdAt:DESC",
    //         cancellationToken: cancellationToken);
    //
    //     return jobsList.Data.FirstOrDefault();
    // }
}
