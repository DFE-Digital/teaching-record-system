using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.Files;

public class BlobStorageSafeFileService : ISafeFileService
{
    private const string UploadsContainerName = "uploads";
    private const string MalwareScanResultTag = "Malware Scanning scan result";
    private const string MalwareScanSuccessValue = "No threats found";
    private const int PollingTimeoutMs = 30000;
    private const int InitialPollingDelayMs = 750;
    private const int PollingPeriodMs = 250;

    private readonly BlobServiceClient _blobServiceClient;
    private readonly IHostEnvironment _hostEnvironment;
    private BlobContainerClient? _blobContainerClient;

    public BlobStorageSafeFileService(
        IAzureClientFactory<BlobServiceClient> blobClientFactory,
        IHostEnvironment hostEnvironment)
    {
        _blobServiceClient = blobClientFactory.CreateClient("safe");
        _hostEnvironment = hostEnvironment;
    }

    public Task<bool> TrySafeUploadAsync(Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride = null)
    {
        fileId = fileIdOverride ?? Guid.NewGuid();
        return TrySafeUploadInternalAsync(stream, contentType, fileId);

        async Task<bool> TrySafeUploadInternalAsync(Stream stream, string? contentType, Guid fileId)
        {
            var blobClient = await GetBlobClientAsync(fileId);
            await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);

            using var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(PollingTimeoutMs);

            var malwareScanResult = await PollForMalwareScanResultAsync(blobClient, cancellationToken.Token);
            if (malwareScanResult != MalwareScanSuccessValue)
            {
                await blobClient.DeleteIfExistsAsync();
            }

            return malwareScanResult == MalwareScanSuccessValue;
        }
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

    private async Task<string?> PollForMalwareScanResultAsync(BlobClient blobClient, CancellationToken cancellationToken)
    {
        await Task.Delay(InitialPollingDelayMs, cancellationToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(PollingPeriodMs));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var blobTags = await blobClient.GetTagsAsync(cancellationToken: cancellationToken);
                if (blobTags.Value.Tags.TryGetValue(MalwareScanResultTag, out var malwareScanResult))
                {
                    return malwareScanResult;
                }
            }

            throw new TimeoutException();
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException();
        }
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
