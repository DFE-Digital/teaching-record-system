namespace TeachingRecordSystem.Core.Services.Files;

public interface IImportFileStorageService
{
    Task<string[]> GetFileNamesAsync(string containerName, string folderName, CancellationToken cancellationToken);
    Task<Stream> GetFileAsync(string containerName, string fileName, CancellationToken cancellationToken);
    Task<Stream> WriteFileAsync(string containerName, string fileName, CancellationToken cancellationToken);
    Task MoveFileAsync(string containerName, string fileName, string targetFolderName, CancellationToken cancellationToken);
}
