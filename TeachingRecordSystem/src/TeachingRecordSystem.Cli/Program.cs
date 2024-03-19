global using System.CommandLine;
global using Microsoft.Extensions.Configuration;
global using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Cli;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly)
    .Build();

var rootCommand = new RootCommand("Development tools for the Teaching Record System.")
{
    Commands.CreateMigrateDbCommand(configuration),
    Commands.CreateMigrateReportingDbCommand(configuration),
    Commands.CreateGenerateReportingDbTableCommand(configuration),
    Commands.CreateCreateAdminCommand(configuration),
    Commands.CreateSyncPersonCommand(configuration),
    Commands.CreateGenerateKeyCommand(configuration),
};

return await rootCommand.InvokeAsync(args);

public partial class Program { }
