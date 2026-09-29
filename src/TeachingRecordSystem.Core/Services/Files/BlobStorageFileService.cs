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

    public async Task<Guid> UploadFileAsync(Stream stream, string? contentType, Guid? fileIdOverride = null, CancellationToken cancellationToken = default)
    {
        var fileId = fileIdOverride ?? Guid.NewGuid();
        var blobClient = await GetBlobClientAsync(fileId, cancellationToken);

        await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null, cancellationToken: cancellationToken);
        return fileId;
    }

    public async Task<bool> UploadFileAsync(string fileName, Stream stream, string? contentType, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(fileName, cancellationToken);
        var response = await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null, cancellationToken: cancellationToken);
        return response.GetRawResponse().Status == 201;
    }

    public async Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(fileId, cancellationToken);

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

    public async Task<string?> TryGetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(fileId, cancellationToken);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        return await GetFileUrlAsync(fileId, expiresAfter, cancellationToken);
    }

    public async Task<Stream> OpenReadStreamAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(fileId, cancellationToken);
        var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
        return stream;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(fileId, cancellationToken);
        var deleted = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        return deleted;
    }

    private Task<BlobClient> GetBlobClientAsync(Guid fileId, CancellationToken cancellationToken)
    {
        return GetBlobClientAsync(fileId.ToString(), cancellationToken);
    }

    private async Task<BlobClient> GetBlobClientAsync(string fileName, CancellationToken cancellationToken)
    {
        await EnsureBlobContainerClientAsync(cancellationToken);
        return _blobContainerClient!.GetBlobClient(fileName);
    }

    private async Task EnsureBlobContainerClientAsync(CancellationToken cancellationToken)
    {
        if (_blobContainerClient is null)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(UploadsContainerName);
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            _blobContainerClient = blobContainerClient;
        }
    }
}
