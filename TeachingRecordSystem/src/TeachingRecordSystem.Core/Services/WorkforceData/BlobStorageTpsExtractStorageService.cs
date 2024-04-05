using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class BlobStorageTpsExtractStorageService(BlobServiceClient blobServiceClient) : ITpsExtractStorageService
{
    private const string TpsExtractsContainerName = "tps-extracts";
    private const string EstablishmentsFolderName = "establishments";
    private const string PendingFolderName = "pending";
    private const string ImportedFolderName = "imported";

    public async Task<string[]> GetPendingImportFileNames(CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(TpsExtractsContainerName);
        var fileNames = new List<string>();
        await GetFileNamesAsync(blobContainerClient, PendingFolderName, true, fileNames, cancellationToken);
        return fileNames.ToArray();
    }

    public async Task<string?> GetPendingEstablishmentImportFileName(CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(TpsExtractsContainerName);
        var fileNames = new List<string>();
        await GetFileNamesAsync(blobContainerClient, EstablishmentsFolderName, true, fileNames, cancellationToken);
        return fileNames?.FirstOrDefault();
    }

    public async Task<Stream> GetFile(string fileName, CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(TpsExtractsContainerName);
        var blobClient = blobContainerClient.GetBlobClient(fileName);
        return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
    }

    public async Task ArchiveFile(string fileName, CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(TpsExtractsContainerName);

        var sourceBlobClient = blobContainerClient.GetBlobClient(fileName);
        if (await sourceBlobClient.ExistsAsync(cancellationToken))
        {
            var fileNameParts = fileName.Split("/");
            var fileNameWithoutFolder = fileNameParts.Last();
            var targetFileName = $"{ImportedFolderName}/{fileNameWithoutFolder}";

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

    private async Task GetFileNamesAsync(BlobContainerClient containerClient, string prefix, bool includeSubfolders, List<string> fileNames, CancellationToken cancellationToken)
    {
        var resultSegment = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/", cancellationToken: cancellationToken).AsPages();

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
                        await GetFileNamesAsync(containerClient, blobhierarchyItem.Prefix, true, fileNames, cancellationToken);
                    }
                }
                else
                {
                    fileNames.Add(blobhierarchyItem.Blob.Name);
                }
            }
        }
    }    
}
