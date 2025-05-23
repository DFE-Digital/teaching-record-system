namespace TeachingRecordSystem.Core.Services.Files;

public interface IFileService
{
    Task<Guid> UploadFileAsync(Stream stream, string? contentType, Guid? fileIdOverride = null);

    Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter);

    Task<Stream> OpenReadStreamAsync(Guid fileId);

    Task<bool> DeleteFileAsync(Guid fileId);
}
