using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class EwcWalesImportJob(BlobServiceClient blobServiceClient, InductionImporter inductionImporter, QtsImporter qtsImporter, ILogger<EwcWalesImportJob> logger)
{
    private const string ProcessedFolder = "ewc/processed";
    private const string PickupFolder = "ewc/pickup";
    private const string StorageContainer = "dqt-integrations";
    public const string JobSchedule = "0 8 * * *";

    private async Task<string[]> GetImportFilesAsync(CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);
        var fileNames = new List<string>();
        var resultSegment = blobContainerClient.GetBlobsByHierarchyAsync(prefix: PickupFolder, delimiter: "", cancellationToken: cancellationToken).AsPages();

        // Enumerate the blobs returned for each page.
        await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
        {
            foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
            {
                if (blobhierarchyItem.IsBlob)
                {
                    fileNames.Add(blobhierarchyItem.Blob.Name);
                }
            }
        }

        return fileNames.ToArray();
    }

    public async Task<Stream> GetDownloadStreamAsync(string fileName)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);
        BlobClient blobClient = containerClient.GetBlobClient($"{fileName}");
        var streamingResult = await blobClient.DownloadStreamingAsync();
        return streamingResult.Value.Content;
    }

    public async Task<long?> ImportAsync(string fileName, StreamReader reader)
    {
        var fileNameParts = fileName.Split("/");
        var fileNameWithoutFolder = fileNameParts.Last();

        if (TryGetImportFileType(fileNameWithoutFolder, out var importType))
        {

            if (importType == EwcWalesImportFileType.Induction)
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
        else
        {
            //file not recognised
            logger.LogError("Import filename must begin with IND or QTS");

            return null;
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var file in await GetImportFilesAsync(cancellationToken))
        {
            using (var downloadStream = await GetDownloadStreamAsync(file))
            using (var reader = new StreamReader(downloadStream))
            {
                await ImportAsync(file, reader);
                await ArchiveFileAsync(file, cancellationToken);
            }
        }
    }

    public async Task ArchiveFileAsync(string fileName, CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);
        var sourceBlobClient = blobContainerClient.GetBlobClient(fileName);
        var fileNameParts = fileName.Split("/");
        var fileNameWithoutFolder = $"{DateTime.Now.ToString("ddMMyyyyHHmm")}-{fileNameParts.Last()}";
        var targetFileName = $"{ProcessedFolder}/{fileNameWithoutFolder}";

        // Acquire a lease to prevent another client modifying the source blob
        var lease = sourceBlobClient.GetBlobLeaseClient();
        await lease.AcquireAsync(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);

        var targetBlobClient = blobContainerClient.GetBlobClient(targetFileName);
        var copyOperation = await targetBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri, cancellationToken: cancellationToken);
        await copyOperation.WaitForCompletionAsync();

        // Release the lease
        var sourceProperties = await sourceBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        if (sourceProperties.Value.LeaseState == LeaseState.Leased)
        {
            await lease.ReleaseAsync(cancellationToken: cancellationToken);
        }

        // Now remove the original blob
        await sourceBlobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
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
