using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateMigrateDbCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var migrateDbCommand = new Command("migrate-db", "Migrate the database to the latest version.")
        {
            connectionStringOption
        };

        migrateDbCommand.SetHandler(
            async (string connectionString) =>
            {
                using var dbContext = TrsDbContext.Create(connectionString);
                await dbContext.Database.MigrateAsync();
            },
            connectionStringOption);

        return migrateDbCommand;
    }
}
