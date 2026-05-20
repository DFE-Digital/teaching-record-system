using System.ComponentModel.DataAnnotations;
using System.Net;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Api;

public class DownloadEvidenceFilesFromBlobStorageHttpHandler(IOptions<EvidenceFilesOptions> optionsAccessor) : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            throw new NotSupportedException();
        }

        var knownBlobStorageAccounts = optionsAccessor.Value.KnownBlobStorageAccounts ?? [];

        foreach (var knownServiceAccount in knownBlobStorageAccounts)
        {
            if (request.RequestUri?.GetLeftPart(UriPartial.Authority) == knownServiceAccount.Uri.TrimEnd('/'))
            {
                var credential = new StorageSharedKeyCredential(knownServiceAccount.AccountName, knownServiceAccount.AccountKey);
                var blobClient = new BlobClient(request.RequestUri, credential);

                try
                {
                    var downloadResponse = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(downloadResponse.Value.Content)
                    };
                }
                catch (RequestFailedException ex)
                {
                    return new HttpResponseMessage((HttpStatusCode)ex.Status);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

public class EvidenceFilesOptions
{
    public KnownBlobStorageAccount[]? KnownBlobStorageAccounts { get; set; }

    public class KnownBlobStorageAccount
    {
        [Required]
        public required string Uri { get; set; }
        [Required]
        public required string AccountName { get; set; }
        [Required]
        public required string AccountKey { get; set; }
    }
}
