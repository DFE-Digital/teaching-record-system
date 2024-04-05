namespace TeachingRecordSystem.Core.Services.WorkforceData;

public interface ITpsExtractStorageService
{
    Task<string[]> GetPendingImportFileNames(CancellationToken cancellationToken);

    Task<string?> GetPendingEstablishmentImportFileName(CancellationToken cancellationToken);

    Task<Stream> GetFile(string fileName, CancellationToken cancellationToken);

    Task ArchiveFile(string fileName, CancellationToken cancellationToken);
}
