namespace TeachingRecordSystem.Core.Services.Files;

public interface ISafeFileService
{
    Task<bool> TrySafeUploadAsync(Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride = null, CancellationToken cancellationToken = default);

    Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadStreamAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}
