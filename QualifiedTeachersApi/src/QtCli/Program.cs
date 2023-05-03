global using System.CommandLine;
global using Microsoft.Extensions.Configuration;
global using Microsoft.PowerPlatform.Dataverse.Client;
using QtCli;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets("QualifiedTeachersApi")
    .Build();

var rootCommand = new RootCommand("Development tools for the Qualified Teachers API.")
{
    Commands.CreateMigrateDbCommand(configuration),
    Commands.CreateMigrateReportingDbCommand(configuration),
    Commands.CreateResetReportingDbJournalCommand(configuration),
    Commands.CreateResetReportingDbChangeCursorCommand(configuration),
    Commands.CreateGenerateReportingDbTableCommand(configuration)
};

return await rootCommand.InvokeAsync(args);
