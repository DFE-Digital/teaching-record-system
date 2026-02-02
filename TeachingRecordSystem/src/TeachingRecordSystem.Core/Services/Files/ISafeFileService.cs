namespace TeachingRecordSystem.Core.Services.Files;

public interface ISafeFileService
{
    Task<bool> TrySafeUploadAsync(Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride = null);

    Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter);

    Task<Stream> OpenReadStreamAsync(Guid fileId);

    Task<bool> DeleteFileAsync(Guid fileId);
}
