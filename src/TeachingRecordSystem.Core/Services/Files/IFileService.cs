namespace TeachingRecordSystem.Core.Services.Files;

public interface IFileService
{
    Task<Guid> UploadFileAsync(Stream stream, string? contentType, Guid? fileIdOverride = null, CancellationToken cancellationToken = default);

    Task<bool> UploadFileAsync(string fileName, Stream stream, string? contentType, CancellationToken cancellationToken = default);

    Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    Task<string?> TryGetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadStreamAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}
