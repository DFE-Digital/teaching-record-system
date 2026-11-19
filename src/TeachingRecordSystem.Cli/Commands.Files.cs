using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
using File = System.IO.File;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateFilesCommand(IConfiguration configuration)
    {
        return new Command("files", "Commands for working with files.")
        {
            CreateDownloadEwcWalesCommand(),
            CreateArchiveEwcWalesCommand()
        };

        Command CreateDownloadEwcWalesCommand()
        {
            var outputDirectoryOption = new Option<string>("--output-directory")
            {
                Required = true,
                Description = "Local directory to download the files into."
            };
            var fileOption = new Option<string?>("--file")
            {
                Description = "Name of a single file in the pickup folder to download. Downloads all files if omitted."
            };
            var accountNameOption = new Option<string>("--storage-account-name") { Required = true };
            var accessKeyOption = new Option<string>("--storage-access-key") { Required = true };

            var configuredAccountName = configuration.GetValue<string>("SftpStorageName");
            if (!string.IsNullOrEmpty(configuredAccountName))
            {
                accountNameOption.DefaultValueFactory = _ => configuredAccountName;
            }

            var configuredAccessKey = configuration.GetValue<string>("SftpStorageAccessKey");
            if (!string.IsNullOrEmpty(configuredAccessKey))
            {
                accessKeyOption.DefaultValueFactory = _ => configuredAccessKey;
            }

            var command = new Command(
                "download-ewc-wales",
                $"Downloads EWC Wales files from the '{EwcWalesImportJob.StorageContainer}/{EwcWalesImportJob.PickupFolder}' blob storage folder to a local directory. Files are not removed from storage.")
            {
                outputDirectoryOption,
                fileOption,
                accountNameOption,
                accessKeyOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var outputDirectory = parseResult.GetRequiredValue(outputDirectoryOption);
                    var fileName = parseResult.GetValue(fileOption);
                    var accountName = parseResult.GetRequiredValue(accountNameOption);
                    var accessKey = parseResult.GetRequiredValue(accessKeyOption);

                    var dataLakeServiceClient = new DataLakeServiceClient(
                        new Uri($"https://{accountName}.dfs.core.windows.net"),
                        new StorageSharedKeyCredential(accountName, accessKey));
                    var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(EwcWalesImportJob.StorageContainer);

                    var paths = new List<string>();
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        paths.Add($"{EwcWalesImportJob.PickupFolder}/{fileName}");
                    }
                    else
                    {
                        await foreach (var pathItem in fileSystemClient.GetPathsAsync(
                            new DataLakeGetPathsOptions { Path = $"{EwcWalesImportJob.PickupFolder}/", Recursive = false }))
                        {
                            if (pathItem.IsDirectory != true)
                            {
                                paths.Add(pathItem.Name);
                            }
                        }
                    }

                    if (paths.Count == 0)
                    {
                        parseResult.InvocationConfiguration.Output.WriteLine("No files found.");
                        return 0;
                    }

                    Directory.CreateDirectory(outputDirectory);

                    foreach (var path in paths)
                    {
                        var localPath = Path.Combine(outputDirectory, Path.GetFileName(path));
                        var fileClient = fileSystemClient.GetFileClient(path);

                        if (!(await fileClient.ExistsAsync()).Value)
                        {
                            parseResult.InvocationConfiguration.Error.WriteLine($"File not found in storage: {path}");
                            return 1;
                        }

                        var readResponse = await fileClient.ReadAsync();
                        await using var source = readResponse.Value.Content;
                        await using var destination = File.Create(localPath);
                        await source.CopyToAsync(destination);

                        parseResult.InvocationConfiguration.Output.WriteLine($"Downloaded {path} to {localPath}");
                    }

                    return 0;
                });

            return command;
        }

        Command CreateArchiveEwcWalesCommand()
        {
            var fileOption = new Option<string>("--file")
            {
                Required = true,
                Description = "Name of the file in the pickup folder to archive."
            };
            var accountNameOption = new Option<string>("--storage-account-name") { Required = true };
            var accessKeyOption = new Option<string>("--storage-access-key") { Required = true };

            var configuredAccountName = configuration.GetValue<string>("SftpStorageName");
            if (!string.IsNullOrEmpty(configuredAccountName))
            {
                accountNameOption.DefaultValueFactory = _ => configuredAccountName;
            }

            var configuredAccessKey = configuration.GetValue<string>("SftpStorageAccessKey");
            if (!string.IsNullOrEmpty(configuredAccessKey))
            {
                accessKeyOption.DefaultValueFactory = _ => configuredAccessKey;
            }

            var command = new Command(
                "archive-ewc-wales",
                $"Archives a single EWC Wales file from the '{EwcWalesImportJob.StorageContainer}/{EwcWalesImportJob.PickupFolder}' folder to the '{EwcWalesImportJob.ArchivedContainer}/{EwcWalesImportJob.ProcessedFolder}' folder.")
            {
                fileOption,
                accountNameOption,
                accessKeyOption
            };

            command.SetAction(
                async (parseResult, cancellationToken) =>
                {
                    var fileName = parseResult.GetRequiredValue(fileOption);
                    var accountName = parseResult.GetRequiredValue(accountNameOption);
                    var accessKey = parseResult.GetRequiredValue(accessKeyOption);

                    var dataLakeServiceClient = new DataLakeServiceClient(
                        new Uri($"https://{accountName}.dfs.core.windows.net"),
                        new StorageSharedKeyCredential(accountName, accessKey));

                    var path = $"{EwcWalesImportJob.PickupFolder}/{Path.GetFileName(fileName)}";
                    var sourceFile = dataLakeServiceClient
                        .GetFileSystemClient(EwcWalesImportJob.StorageContainer)
                        .GetFileClient(path);

                    if (!(await sourceFile.ExistsAsync(cancellationToken)).Value)
                    {
                        parseResult.InvocationConfiguration.Error.WriteLine($"File not found in storage: {path}");
                        return 1;
                    }

                    await EwcWalesImportJob.ArchiveFileAsync(dataLakeServiceClient, path, cancellationToken);

                    parseResult.InvocationConfiguration.Output.WriteLine($"Archived {path}");
                    return 0;
                });

            return command;
        }
    }
}


