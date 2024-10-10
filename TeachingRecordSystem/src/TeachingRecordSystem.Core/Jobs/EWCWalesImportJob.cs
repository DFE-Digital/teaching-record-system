using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

namespace TeachingRecordSystem.Core.Jobs;

public class EwcWalesImportJob(ICrmQueryDispatcher crmQueryDispatcher, BlobServiceClient blobServiceClient, InductionImporter inductionImporter, QtsImporter qtsImporter)
{
    private readonly string _processedFolder = "ewc/processed";
    private readonly string _pickupFolder = "ewc/pickup";
    private readonly string _storageContainer = "legacy-integrations-import";


    private async Task<string[]> GetImportFilesAsync(BlobContainerClient containerClient, string prefix, bool includeSubfolders, CancellationToken cancellationToken)
    {
        var fileNames = new List<string>();
        var resultSegment = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: _pickupFolder, cancellationToken: cancellationToken).AsPages();

        // Enumerate the blobs returned for each page.
        await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
        {
            foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
            {
                // A hierarchical listing may return both virtual directories and blobs.
                if (blobhierarchyItem.IsPrefix)
                {
                    if (includeSubfolders)
                    {
                        // Call recursively with the prefix to traverse the virtual directory.
                        var subfolderFileNames = await GetFileNamesAsync(containerClient, blobhierarchyItem.Prefix, true, cancellationToken);
                        if (subfolderFileNames != null)
                        {
                            fileNames.AddRange(subfolderFileNames);
                        }
                    }
                }
                else
                {
                    fileNames.Add(blobhierarchyItem.Blob.Name);
                }
            }
        }

        return fileNames.ToArray();
    }

    private async Task<string[]> GetFileNamesAsync(BlobContainerClient containerClient, string prefix, bool includeSubfolders, CancellationToken cancellationToken)
    {
        var fileNames = new List<string>();
        var resultSegment = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: _pickupFolder, cancellationToken: cancellationToken).AsPages();

        // Enumerate the blobs returned for each page.
        await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
        {
            foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
            {
                // A hierarchical listing may return both virtual directories and blobs.
                if (blobhierarchyItem.IsPrefix)
                {
                    if (includeSubfolders)
                    {
                        // Call recursively with the prefix to traverse the virtual directory.
                        var subfolderFileNames = await GetFileNamesAsync(containerClient, blobhierarchyItem.Prefix, true, cancellationToken);
                        if (subfolderFileNames != null)
                        {
                            fileNames.AddRange(subfolderFileNames);
                        }
                    }
                }
                else
                {
                    fileNames.Add(blobhierarchyItem.Blob.Name);
                }
            }
        }

        return fileNames.ToArray();
    }

    public async Task<MemoryStream> GetDownloadStreamAsync(string fileName)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_storageContainer);
        BlobClient blobClient = containerClient.GetBlobClient($"{fileName}");
        var downloadStream = new MemoryStream();
        await blobClient.DownloadToAsync(downloadStream);
        downloadStream.Position = 0;
        return downloadStream;
    }

    public async Task<(int TotalCount, int SuccessCount, int DuplicateCount, int FailureCount, string FailureMessage, Guid? IntegrationTransactionId)> ImportAsync(string fileName, StreamReader reader)
    {
        var fileNameParts = fileName.Split("/");
        var fileNameWithoutFolder = fileNameParts.Last();
        var integrationJob = new CreateIntegrationTransactionQuery()
        {
            StartDate = DateTime.Now,
            TypeId = (int)dfeta_IntegrationInterface.GTCWalesImport,
            FileName = fileName
        };

        var integrationId = await crmQueryDispatcher.ExecuteQueryAsync(integrationJob);
        TryGetImportFileType(fileNameWithoutFolder, out var importType);
        var (totalCount, successCount, duplicateCount, failureCount, failureMessage) = (0, 0, 0, 0, "");

        if (importType == EwcWalesImportFileType.Induction)
        {
            var results = await inductionImporter.ImportAsync(reader, integrationId, fileNameWithoutFolder);
            totalCount = results.TotalCount;
            successCount = results.SuccessCount;
            duplicateCount = results.DuplicateCount;
            failureCount = results.FailureCount;
            failureMessage = results.FailureMessage;
        }
        else if (importType == EwcWalesImportFileType.Qualification)
        {
            var results = await qtsImporter.ImportAsync(reader, integrationId, fileNameWithoutFolder);
            totalCount = results.TotalCount;
            successCount = results.SuccessCount;
            duplicateCount = results.DuplicateCount;
            failureCount = results.FailureCount;
            failureMessage = results.FailureMessage;
        }
        else
        {
            totalCount = 0;
            successCount = 0;
            duplicateCount = 0;
            failureCount = 0;
            failureMessage = "Import filename must begin with IND or QTS";
        }

        var updateIntegrationTransactionQuery = new UpdateIntegrationTransactionQuery()
        {
            IntegrationTransactionId = integrationId,
            EndDate = DateTime.Now,
            TotalCount = totalCount,
            SuccessCount = successCount,
            DuplicateCount = duplicateCount,
            FailureCount = failureCount,
            FailureMessage = failureMessage
        };

        using var txn = crmQueryDispatcher.CreateTransactionRequestBuilder();
        txn.AppendQuery(updateIntegrationTransactionQuery);
        await txn.ExecuteAsync();
        return (totalCount, successCount, duplicateCount, failureCount, failureMessage, integrationId);
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_storageContainer);
        foreach (var file in await GetImportFilesAsync(blobContainerClient, "", true, cancellationToken))
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
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_storageContainer);

        var sourceBlobClient = blobContainerClient.GetBlobClient(fileName);
        if (await sourceBlobClient.ExistsAsync(cancellationToken))
        {
            var fileNameParts = fileName.Split("/");
            var fileNameWithoutFolder = fileNameParts.Last();
            var targetFileName = $"{_processedFolder}/{fileNameWithoutFolder}";

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
    }

    public bool TryGetImportFileType(string fileName, out EwcWalesImportFileType fileType) =>
        (fileType = fileName switch
        {
            var f when f.StartsWith("Ind", StringComparison.OrdinalIgnoreCase) => EwcWalesImportFileType.Induction,
            var f when f.StartsWith("QTS", StringComparison.OrdinalIgnoreCase) => EwcWalesImportFileType.Qualification,
            _ => EwcWalesImportFileType.Unknown
        }) != EwcWalesImportFileType.Unknown;
}
