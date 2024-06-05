namespace TeachingRecordSystem.Core.Services.Files;

public interface IFileService
{
    Task<Guid> UploadFile(Stream stream, string? contentType);

    Task<string> GetFileUrl(Guid fileId, TimeSpan expiresAfter);

    Task<Stream> OpenReadStream(Guid fileId);

    Task DeleteFile(Guid fileId);
}
