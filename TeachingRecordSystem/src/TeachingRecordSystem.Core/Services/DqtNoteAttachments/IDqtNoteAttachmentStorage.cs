using AngleSharp.Io;

namespace TeachingRecordSystem.Core.Services.DqtNoteAttachments;

public interface IDqtNoteAttachmentStorage
{
    public Task<(byte[] AttachmentBytes, string MimeType)?> GetAttachmentAsync(string fileName);
    public Task<bool> CreateAttachmentAsync(byte[] attachmentBytes, string fileName, string? mimeType = null);
    public Task<bool> DeleteAttachmentAsync(string fileName);
}
