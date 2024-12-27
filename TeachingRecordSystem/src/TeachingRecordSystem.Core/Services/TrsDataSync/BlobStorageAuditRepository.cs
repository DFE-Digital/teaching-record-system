using System.Runtime.Serialization;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Crm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public class BlobStorageAuditRepository(BlobServiceClient blobServiceClient) : IAuditRepository
{
    private const string ContainerName = "dqtaudits";

    private static readonly DataContractSerializer _serializer =
        new DataContractSerializer(
            typeof(AuditDetailCollection),
            knownTypes: [
                typeof(Audit),
                typeof(Contact),
                typeof(dfeta_induction)
            ]);

    public async Task<AuditDetailCollection?> GetAuditDetailAsync(string entityLogicalName, Guid id)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(GetBlobName(entityLogicalName, id));

        try
        {
            var response = await blobClient.DownloadContentAsync();
            await using var contentStream = response.Value.Content.ToStream();
            return (AuditDetailCollection)_serializer.ReadObject(contentStream)!;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task SetAuditDetailAsync(string entityLogicalName, Guid id, AuditDetailCollection auditDetailCollection)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(GetBlobName(entityLogicalName, id));

        using var ms = new MemoryStream();
        _serializer.WriteObject(ms, auditDetailCollection);
        ms.Seek(0L, SeekOrigin.Begin);

        await blobClient.UploadAsync(ms, overwrite: true);
    }

    private static string GetBlobName(string entityLogicalName, Guid id) => $"{entityLogicalName}/{id:N}.xml";
}
