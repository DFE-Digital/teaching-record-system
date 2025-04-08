using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TeachingRecordSystem.Core.Services.DqtNoteAttachments;

public class BlobStorageDqtNoteAttachmentStorage : IDqtNoteAttachmentStorage
{
    private const string StorageContainer = "dqt-note-attachments";
    private readonly BlobContainerClient _blobContainerClient;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageDqtNoteAttachmentStorage(BlobServiceClient blobServiceClient)
    {
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(StorageContainer);
        _blobServiceClient = blobServiceClient;
    }

    public async Task<bool> CreateAttachmentAsync(byte[] attachmentBytes, string fileName, string? mimetype = null)
    {
        var httpHeaders = new BlobHttpHeaders
        {
            ContentType = mimetype
        };

        var blobClient = _blobContainerClient.GetBlobClient(fileName);

        var binaryData = BinaryData.FromBytes(attachmentBytes);
        await blobClient.UploadAsync(binaryData, new BlobUploadOptions() { HttpHeaders = httpHeaders });

        return true;
    }

    public async Task<(byte[] AttachmentBytes, string MimeType)?> GetAttachmentAsync(string file)
    {
        var blobClient = _blobContainerClient.GetBlobClient(file);
        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();
        return (response.Value.Content.ToArray(), response.Value.Details.ContentType);
    }

    public async Task<bool> DeleteAttachmentAsync(string fileName)
    {
        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        var isDeleted = await blobClient.DeleteIfExistsAsync();
        return isDeleted;
    }
}
