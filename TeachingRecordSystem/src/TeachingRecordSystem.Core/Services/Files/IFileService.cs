namespace TeachingRecordSystem.Core.Services.Files;

public interface IFileService
{
    Task<Guid> UploadFileAsync(Stream stream, string? contentType);

    Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter);

    Task<Stream> OpenReadStreamAsync(Guid fileId);

    Task DeleteFileAsync(Guid fileId);
}
