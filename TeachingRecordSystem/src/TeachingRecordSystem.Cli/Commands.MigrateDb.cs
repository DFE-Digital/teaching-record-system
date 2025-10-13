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

                // Ensure we've got squashed migration recorded
                var migrationsAssembly = dbContext.Database.GetService<IMigrationsAssembly>();
                var initialMigration = migrationsAssembly.Migrations.OrderBy(k => k.Key).First();
                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
                var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();

                if (appliedMigrations.Length != 0 && pendingMigrations.Contains(initialMigration.Key))
                {
                    using var txn = await dbContext.Database.BeginTransactionAsync();
                    var historyRepository = dbContext.GetService<IHistoryRepository>();
                    await using var @lock = await historyRepository.AcquireDatabaseLockAsync();

                    // Add the squashed migration to the history table
                    var insertScript = historyRepository.GetInsertScript(new HistoryRow(initialMigration.Key, ProductInfo.GetVersion()));
                    await dbContext.Database.ExecuteSqlRawAsync(insertScript);

                    // Remove all migrations that don't exist any more (i.e. have been squashed)
                    foreach (var appliedMigration in appliedMigrations)
                    {
                        if (!migrationsAssembly.Migrations.ContainsKey(appliedMigration))
                        {
                            var deleteScript = historyRepository.GetDeleteScript(appliedMigration);
                            await dbContext.Database.ExecuteSqlRawAsync(deleteScript);
                        }
                    }

                    await txn.CommitAsync();
                }

                var migrator = dbContext.GetService<IMigrator>();
                await migrator.MigrateAsync(targetMigration);

                // Ensure the user has the replication permission
                var user = new NpgsqlConnectionStringBuilder(connectionString).Username;
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                await dbContext.Database.ExecuteSqlRawAsync($"alter user {user} with replication");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            },
            connectionStringOption,
            targetMigrationOption);

        return migrateDbCommand;
    }
}
