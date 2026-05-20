namespace TeachingRecordSystem.Core.Services.WorkforceData;

public interface ITpsExtractStorageService
{
    Task<string[]> GetPendingImportFileNamesAsync(CancellationToken cancellationToken);

    Task<string?> GetPendingEstablishmentImportFileNameAsync(CancellationToken cancellationToken);

    Task<Stream> GetFileAsync(string fileName, CancellationToken cancellationToken);

    Task ArchiveFileAsync(string fileName, CancellationToken cancellationToken);
}
