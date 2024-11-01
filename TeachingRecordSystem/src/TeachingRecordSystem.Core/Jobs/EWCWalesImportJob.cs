using Azure.Storage.Blobs;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.EWCWalesImport;

namespace TeachingRecordSystem.Core.Jobs;

public class EWCWalesImportJob(ICrmQueryDispatcher crmQueryDispatcher, BlobServiceClient blobServiceClient, InductionImporter inductionImporter, QTSImporter qtsImporter)
{
    private readonly string _processedFolder = "processed";
    private readonly string _pickupFolder = "pickup";
    private readonly string _storageContainer = "legacy-integrations-import";

    public string[] GetImportFiles()
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_storageContainer);
        var files = containerClient.GetBlobs().Select(x => x.Name).ToArray();
        return files;
    }

    public async Task<MemoryStream> GetDownloadStream(string fileName)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_storageContainer);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        using (var downloadStream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(downloadStream);
            downloadStream.Position = 0;
            return downloadStream;
        }
    }

    public async Task Import(string fileName, StreamReader reader)
    {
        var integrationJob = new CreateIntegrationTransactionQuery()
        {
            StartDate = DateTime.Now,
            TypeId = (int)dfeta_IntegrationInterface.GTCWalesImport
        };
        var integrationId = await crmQueryDispatcher.ExecuteQuery(integrationJob);
        var importType = GetImporFileType(fileName);

        if (importType == EWCWalesImportFileType.Induction)
        {
            await inductionImporter.Import(reader, integrationId);
        }
        else if (importType == EWCWalesImportFileType.Qualification)
        {
            await qtsImporter.Import(reader, integrationId);
        }
    }

    public StreamReader GetStreamReader(byte[] filecontents)
    {
        var stream = new MemoryStream(filecontents);
        var reader = new StreamReader(stream);
        return reader;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        foreach (var file in GetImportFiles())
        {
            using (var downloadStream = await GetDownloadStream(file))
            using(var reader = GetStreamReader(downloadStream.ToArray()))
            {
                await Import(file, reader);
            }
        }
    }

    public EWCWalesImportFileType GetImporFileType(string fileName) => fileName.ToLower() switch
    {
        var filename when filename.Contains("Ind", StringComparison.OrdinalIgnoreCase) => EWCWalesImportFileType.Induction,
        var filename when filename.Contains("QTS", StringComparison.OrdinalIgnoreCase) => EWCWalesImportFileType.Qualification,
        _ => throw new InvalidOperationException("Not matching EWCWales Filename")
    };
}





