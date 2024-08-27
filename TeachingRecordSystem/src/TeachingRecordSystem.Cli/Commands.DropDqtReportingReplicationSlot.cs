using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateDropDqtReportingReplicationSlotCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var command = new Command("drop-dqt-reporting-replication-slot", "Drops the logical replication slot for the DQT Reporting Service.")
        {
            connectionStringOption
        };

        command.SetHandler(
            async (string connectionString) =>
            {
                using var dbContext = TrsDbContext.Create(connectionString, commandTimeout: (int)TimeSpan.FromMinutes(10).TotalSeconds);

                // Ensure the user has the replication permission
                var user = new NpgsqlConnectionStringBuilder(connectionString).Username;
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                await dbContext.Database.ExecuteSqlRawAsync($"alter user {user} with replication");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.

                await dbContext.Database.ExecuteSqlRawAsync(
                    $"select pg_drop_replication_slot('{DqtReportingOptions.DefaultTrsDbReplicationSlotName}');");
            },
            connectionStringOption);

        return command;
    }
}
