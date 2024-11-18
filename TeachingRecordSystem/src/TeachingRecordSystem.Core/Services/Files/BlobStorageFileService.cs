using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace TeachingRecordSystem.Core.Services.Files;

public class BlobStorageFileService : IFileService
{
    private const string UploadsContainerName = "uploads";
    private readonly BlobServiceClient _blobServiceClient;
    private BlobContainerClient? _blobContainerClient;

    public BlobStorageFileService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Guid> UploadFileAsync(Stream stream, string? contentType)
    {
        var fileId = Guid.NewGuid();
        var blobClient = await GetBlobClientAsync(fileId);

        await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);
        return fileId;
    }

    public async Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter)
    {
        var blobClient = await GetBlobClientAsync(fileId);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = UploadsContainerName,
            BlobName = fileId.ToString(),
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiresAfter),
            Protocol = SasProtocol.Https
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task<Stream> OpenReadStreamAsync(Guid fileId)
    {
        var blobClient = await GetBlobClientAsync(fileId);
        var stream = await blobClient.OpenReadAsync();
        return stream;
    }

    public async Task DeleteFileAsync(Guid fileId)
    {
        var blobClient = await GetBlobClientAsync(fileId);
        await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
    }

    private async Task<BlobClient> GetBlobClientAsync(Guid fileId)
    {
        await EnsureBlobContainerClientAsync();
        return _blobContainerClient!.GetBlobClient(fileId.ToString());
    }

    private async Task EnsureBlobContainerClientAsync()
    {
        if (_blobContainerClient is null)
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(UploadsContainerName);
            await _blobContainerClient.CreateIfNotExistsAsync();
        }
    }
}
