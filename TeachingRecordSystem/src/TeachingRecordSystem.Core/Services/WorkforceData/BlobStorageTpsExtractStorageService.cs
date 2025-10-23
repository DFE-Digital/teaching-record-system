using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class BlobStorageTpsExtractStorageService(BlobStorageImportFileStorageService fileStorageService) : ITpsExtractStorageService
{
    private const string TpsExtractsContainerName = "tps-extracts";
    private const string EstablishmentsFolderName = "establishments";
    private const string PendingFolderName = "pending";
    private const string ImportedFolderName = "imported";

    public Task<string[]> GetPendingImportFileNamesAsync(CancellationToken cancellationToken) =>
        fileStorageService.GetFileNamesAsync(TpsExtractsContainerName, PendingFolderName, cancellationToken);

    public async Task<string?> GetPendingEstablishmentImportFileNameAsync(CancellationToken cancellationToken)
    {
        var fileNames = await fileStorageService.GetFileNamesAsync(TpsExtractsContainerName, EstablishmentsFolderName, cancellationToken);

        return fileNames.FirstOrDefault();
    }

    public Task<Stream> GetFileAsync(string fileName, CancellationToken cancellationToken) =>
        fileStorageService.GetFileAsync(TpsExtractsContainerName, fileName, cancellationToken);

    public Task ArchiveFileAsync(string fileName, CancellationToken cancellationToken) =>
        fileStorageService.MoveFileAsync(TpsExtractsContainerName, fileName, ImportedFolderName, cancellationToken);
}
