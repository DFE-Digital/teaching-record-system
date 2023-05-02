using QualifiedTeachersApi.Services.DqtReporting;

namespace QtCli;

public static partial class Commands
{
    public static Command CreateResetReportingDbJournalCommand(IConfiguration configuration)
    {
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

        var configuredConnectionString = configuration["DqtReporting:ReportingDbConnectionString"];
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var migrateDbCommand = new Command("reset-reporting-db-journal", "Reset the migrations journal table to Initial.sql.")
        {
            connectionStringOption
        };

        migrateDbCommand.SetHandler(
            (string connectionString) =>
            {
                var migrator = new Migrator(connectionString);
                migrator.ResetJournal("Initial.sql");
            },
            connectionStringOption);

        return migrateDbCommand;
    }
}
