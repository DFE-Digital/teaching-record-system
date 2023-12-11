using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateMigrateReportingDbCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

        var configuredConnectionString = configuration["DqtReporting:ReportingDbConnectionString"];
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var migrateDbCommand = new Command("migrate-reporting-db", "Migrate the SQL Server reporting database to the latest version.")
        {
            connectionStringOption
        };

        migrateDbCommand.SetHandler(
            (string connectionString) =>
            {
                var migrator = new Migrator(connectionString);
                migrator.MigrateDb();
            },
            connectionStringOption);

        return migrateDbCommand;
    }
}
