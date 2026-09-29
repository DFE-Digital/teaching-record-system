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

    public Task<bool> TrySafeUploadAsync(Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride = null, CancellationToken cancellationToken = default)
    {
        fileId = fileIdOverride ?? Guid.NewGuid();
        return TrySafeUploadInternalAsync(stream, contentType, fileId);

        async Task<bool> TrySafeUploadInternalAsync(Stream stream, string? contentType, Guid fileId)
        {
            var blobClient = await GetBlobClientAsync(fileId, cancellationToken);
            await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null, cancellationToken: cancellationToken);

            var malwareScanResult = await PollForMalwareScanResultAsync(blobClient, cancellationToken);
            if (malwareScanResult != MalwareScanSuccessValue)
            {
                // Don't leave a file that failed the scan behind just because the caller has gone away
                await blobClient.DeleteIfExistsAsync(cancellationToken: CancellationToken.None);
            }

            return malwareScanResult == MalwareScanSuccessValue;
        }
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

    private async Task<string?> PollForMalwareScanResultAsync(BlobClient blobClient, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(PollingTimeoutMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(PollingPeriodMs));

        try
        {
            await Task.Delay(InitialPollingDelayMs, timeoutCts.Token);

            while (await timer.WaitForNextTickAsync(timeoutCts.Token))
            {
                var blobTags = await blobClient.GetTagsAsync(cancellationToken: timeoutCts.Token);
                if (blobTags.Value.Tags.TryGetValue(MalwareScanResultTag, out var malwareScanResult))
                {
                    return malwareScanResult;
                }
            }

            throw new TimeoutException();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
    }

    private async Task<BlobClient> GetBlobClientAsync(Guid fileId, CancellationToken cancellationToken)
    {
        await EnsureBlobContainerClientAsync(cancellationToken);
        return _blobContainerClient!.GetBlobClient(fileId.ToString());
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
