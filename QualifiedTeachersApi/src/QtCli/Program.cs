global using System.CommandLine;
global using Microsoft.Extensions.Configuration;
using QtCli;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets("QualifiedTeachersApi")
    .Build();

var rootCommand = new RootCommand("Development tools for the Qualified Teachers API.")
{
    Commands.CreateMigrateDbCommand(configuration),
    Commands.CreateMigrateReportingDbCommand(configuration),
    Commands.CreateResetReportingDbJournalCommand(configuration),
};

return await rootCommand.InvokeAsync(args);
