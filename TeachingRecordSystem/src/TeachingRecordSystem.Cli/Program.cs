using TeachingRecordSystem.Cli;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets(typeof(Program).Assembly)
    .Build();

var rootCommand = new RootCommand("Development tools for the Teaching Record System.")
{
    Commands.CreateMigrateDbCommand(configuration),
    Commands.CreateCreateAdminCommand(configuration),
    Commands.CreateGenerateKeyCommand(configuration),
    Commands.CreateDropDqtReportingReplicationSlotCommand(configuration),
    Commands.CreateGenerateWebhookSignatureCertificateCommand(configuration),
    Commands.CreateWebhookEndpointCommand(configuration),
    Commands.CreateAddTrnRangeCommand(configuration),
};

return await rootCommand.InvokeAsync(args);
