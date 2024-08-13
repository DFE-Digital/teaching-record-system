using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateMigrateDbCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };
        var targetMigrationOption = new Option<string>("--target-migration") { IsRequired = false };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var migrateDbCommand = new Command("migrate-db", "Migrate the database to the latest version.")
        {
            connectionStringOption,
            targetMigrationOption
        };

        migrateDbCommand.SetHandler(
            async (string connectionString, string? targetMigration) =>
            {
                using var dbContext = TrsDbContext.Create(connectionString, commandTimeout: (int)TimeSpan.FromMinutes(10).TotalSeconds);

                // Ensure the user has the replication permission
                var user = new NpgsqlConnectionStringBuilder(connectionString).Username;
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                await dbContext.Database.ExecuteSqlRawAsync($"alter user {user} with replication");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.

                await dbContext.GetService<IMigrator>().MigrateAsync(targetMigration);
            },
            connectionStringOption,
            targetMigrationOption);

        return migrateDbCommand;
    }
}
