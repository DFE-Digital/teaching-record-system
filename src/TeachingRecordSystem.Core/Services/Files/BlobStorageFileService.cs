using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Files;

public class BlobStorageFileService : IFileService
{
    private const string UploadsContainerName = "uploads";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly IHostEnvironment _hostEnvironment;
    private BlobContainerClient? _blobContainerClient;

    public BlobStorageFileService(BlobServiceClient blobServiceClient, IHostEnvironment hostEnvironment)
    {
        _blobServiceClient = blobServiceClient;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<Guid> UploadFileAsync(Stream stream, string? contentType, Guid? fileIdOverride = null)
    {
        var fileId = fileIdOverride ?? Guid.NewGuid();
        var blobClient = await GetBlobClientAsync(fileId);

        await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);
        return fileId;
    }

    public async Task<bool> UploadFileAsync(string fileName, Stream stream, string? contentType)
    {
        var blobClient = await GetBlobClientAsync(fileName);
        var response = await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);
        return response.GetRawResponse().Status == 201;
    }

    public async Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter)
    {
        var blobClient = await GetBlobClientAsync(fileId);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = UploadsContainerName,
            BlobName = fileId.ToString(),
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiresAfter),
            Protocol = _hostEnvironment.IsDevelopment() ? SasProtocol.HttpsAndHttp : SasProtocol.Https
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

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var blobClient = await GetBlobClientAsync(fileId);
        var deleted = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        return deleted;
    }

    private Task<BlobClient> GetBlobClientAsync(Guid fileId)
    {
        return GetBlobClientAsync(fileId.ToString());
    }

    private async Task<BlobClient> GetBlobClientAsync(string fileName)
    {
        await EnsureBlobContainerClientAsync();
        return _blobContainerClient!.GetBlobClient(fileName);
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
