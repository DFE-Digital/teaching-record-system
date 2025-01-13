using System.Runtime.Serialization;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public class BlobStorageAuditRepository(BlobServiceClient blobServiceClient, ILogger<BlobStorageAuditRepository> logger) : IAuditRepository
{
    private const string ContainerName = "dqtaudits";

    private static readonly DataContractSerializer _serializer =
        new DataContractSerializer(
            typeof(AuditDetailCollection),
            knownTypes: [
                typeof(Audit),
                typeof(Contact),
                typeof(dfeta_induction),
                typeof(dfeta_initialteachertraining),
                typeof(dfeta_qtsregistration)
            ]);

    public async Task<AuditDetailCollection?> GetAuditDetailAsync(string entityLogicalName, string primaryIdAttribute, Guid id)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(GetBlobName(entityLogicalName, id));

        try
        {
            var response = await blobClient.DownloadContentAsync();
            await using var contentStream = response.Value.Content.ToStream();

            var auditDetail = (AuditDetailCollection)_serializer.ReadObject(contentStream)!;

            // Deserialization wrongly sets the Primary Id attribute to be non null because it gets it from the non-null Id attribute - we need to unset this for NewValue and OldValue
            foreach (var item in auditDetail.AuditDetails.OfType<AttributeAuditDetail>().Where(a => a.AuditRecord.ToEntity<Audit>().Action != Audit_Action.Merge))
            {
                item.NewValue.Attributes.Remove(primaryIdAttribute);
                item.OldValue.Attributes.Remove(primaryIdAttribute);
            }

            return auditDetail;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error reading audit detail for {entityLogicalName} with id {id}");
            throw;
        }
    }

    public async Task<bool> HaveAuditDetailAsync(string entityLogicalName, Guid id)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(GetBlobName(entityLogicalName, id));
        return await blobClient.ExistsAsync();
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
