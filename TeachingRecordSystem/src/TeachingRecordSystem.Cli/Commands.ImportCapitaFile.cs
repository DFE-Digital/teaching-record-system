using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using File = System.IO.File;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateImportCapitaFileCommand(IConfiguration configuration)
    {
        var fileOption = new Option<string>("--file", "--file") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };
        var capitaUserIdOption = new Option<Guid>("--capita-user-id") { Required = true };
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("import-capita-file", "imports local capita file, skipping the sftp.")
        {
            fileOption,
            capitaUserIdOption,
            connectionStringOption
        };

        command.SetAction(async parseResult =>
        {
            var fileName = parseResult.GetRequiredValue(fileOption);
            var connectionString = parseResult.GetRequiredValue(connectionStringOption);
            var capitaUserId = parseResult.GetRequiredValue(capitaUserIdOption);
            var capitaUserOptions = Options.Create(new CapitaTpsUserOption
            {
                CapitaTpsUserId = capitaUserId
            });

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(
                    $"The specified file does not exist: '{fileName}'.",
                    fileName);
            }

            var services = new ServiceCollection()
                .AddClock()
                .AddLogging()
                .AddDatabase(connectionString)
                .AddEventPublisher()
                .AddPersonService()
                .AddSupportTaskService()
                .AddIdentityApi(configuration)
                .AddTrnRequestService(configuration)
                .BuildServiceProvider();

            var notNeededDatalakeClient = new DataLakeServiceClient(
                new Uri("https://notused.invalid"),
                new DefaultAzureCredential());

            var job = ActivatorUtilities.CreateInstance<CapitaImportJob>(services,
                notNeededDatalakeClient,
                capitaUserOptions);

            using var stream = File.OpenRead(fileName);
            using var reader = new StreamReader(stream);
            await job.ImportAsync(reader, fileName);
        });

        return command;
    }
}
