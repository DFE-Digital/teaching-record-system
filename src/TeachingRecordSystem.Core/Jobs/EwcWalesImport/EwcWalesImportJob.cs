using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class EwcWalesImportJob([FromKeyedServices("sftpstorage")] DataLakeServiceClient dataLakeServiceClient, InductionImporter inductionImporter, QtsImporter qtsImporter, ILogger<EwcWalesImportJob> logger)
{
    private const string ProcessedFolder = "ewc/processed";
    public const string PickupFolder = "pickup";
    public const string StorageContainer = "ewc-integrations";
    public const string JobSchedule = "0 8 * * *";
    public const string ArchivedContainer = "archived-integration-transactions";

    private async Task<string[]> GetImportFilesAsync(CancellationToken cancellationToken)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        var fileNames = new List<string>();

        await foreach (var pathItem in fileSystemClient.GetPathsAsync(new DataLakeGetPathsOptions { Path = $"{PickupFolder}/", Recursive = false }, cancellationToken))
        {
            // Only add files, skip directories
            if (pathItem.IsDirectory == false)
            {
                fileNames.Add(pathItem.Name);
            }
        }

        return fileNames.ToArray();
    }

    public async Task<Stream> GetDownloadStreamAsync(string fileName)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        var fileClient = fileSystemClient.GetFileClient(fileName);

        var readResponse = await fileClient.ReadAsync();
        return readResponse.Value.Content;
    }

    public async Task<long?> ImportAsync(string fileName, EwcWalesImportFileType fileType, StreamReader reader)
    {
        var fileNameWithoutFolder = fileName.Split("/").Last();

        if (fileType == EwcWalesImportFileType.Induction)
        {
            var result = await inductionImporter.ImportAsync(reader, fileNameWithoutFolder);
            return result.IntegrationTransactionId;
        }
        else
        {
            var result = await qtsImporter.ImportAsync(reader, fileNameWithoutFolder);
            return result.IntegrationTransactionId;
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var file in await GetImportFilesAsync(cancellationToken))
        {
            // Files that aren't Induction or QTS imports are left in the pickup folder (e.g. for processing via the CLI).
            if (!TryGetImportFileType(file.Split('/').Last(), out var fileType) || fileType is null)
            {
                logger.LogWarning("Skipping file '{FileName}': import filename must begin with IND or QTS", file);
                continue;
            }

            using (var downloadStream = await GetDownloadStreamAsync(file))
            using (var reader = new StreamReader(downloadStream))
            {
                await ImportAsync(file, fileType.Value, reader);
                await ArchiveFileAsync(file, cancellationToken);
            }
        }
    }

    public async Task ArchiveFileAsync(string fileName, CancellationToken cancellationToken)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(StorageContainer);
        var arhivedFileSystemClient = dataLakeServiceClient.GetFileSystemClient(ArchivedContainer);
        var sourceFile = fileSystemClient.GetFileClient(fileName);

        var fileNameParts = fileName.Split('/');
        var fileNameWithoutFolder = $"{DateTime.UtcNow:ddMMyyyyHHmm}-{fileNameParts.Last()}";
        var targetPath = $"{ProcessedFolder}/{fileNameWithoutFolder}";
        var targetFile = arhivedFileSystemClient.GetFileClient(targetPath);

        await targetFile.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // Read the source file
        var readResponse = await sourceFile.ReadAsync(cancellationToken: cancellationToken);
        await using var sourceStream = readResponse.Value.Content;

        await using var memory = new MemoryStream();
        await sourceStream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        await targetFile.AppendAsync(memory, offset: 0, cancellationToken: cancellationToken);
        await targetFile.FlushAsync(memory.Length, cancellationToken: cancellationToken);

        // Delete the original file
        await sourceFile.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public bool TryGetImportFileType(string fileName, out EwcWalesImportFileType? fileType)
    {
        fileType = fileName switch
        {
            var f when f.StartsWith("Ind", StringComparison.OrdinalIgnoreCase) => EwcWalesImportFileType.Induction,
            var f when f.StartsWith("QTS", StringComparison.OrdinalIgnoreCase) => EwcWalesImportFileType.Qualification,
            _ => null
        };

        return fileType is not null;
    }
}
