using TeachingRecordSystem.Cli;

var configuration = new ConfigurationManager();
configuration
    .AddEnvironmentVariables()
    .AddUserSecrets(typeof(Program).Assembly);

var rootCommand = new RootCommand("Development tools for the Teaching Record System.")
{
    Commands.CreateMigrateDbCommand(configuration),
    Commands.CreateCreateAdminCommand(configuration),
    Commands.CreateGenerateKeyCommand(configuration),
    Commands.CreateDropDqtReportingReplicationSlotCommand(configuration),
    Commands.CreateGenerateWebhookSignatureCertificateCommand(configuration),
    Commands.CreateWebhookEndpointCommand(configuration),
    Commands.CreateAddTrnRangeCommand(configuration),
    Commands.CreateDeleteSupportTaskCommand(configuration),
    Commands.CreateResetTrnRequestCommand(configuration),
    Commands.CreateDeployAnalyticsCommand(configuration)
};

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
