using Azure.Storage.Blobs;
using Medallion.Threading;
using Medallion.Threading.Azure;
using Medallion.Threading.FileSystem;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Infrastructure;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddDistributedLocks(this IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            var containerName = builder.Configuration.GetRequiredValue("DistributedLockContainerName");

            builder.Services.AddSingleton<IDistributedLockProvider>(sp =>
            {
                var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                return new AzureBlobLeaseDistributedSynchronizationProvider(blobContainerClient);
            });
        }
        else
        {
            var lockFileDirectory = Path.Combine(Path.GetTempPath(), "qtlocks");
            builder.Services.AddSingleton<IDistributedLockProvider>(new FileDistributedSynchronizationProvider(new DirectoryInfo(lockFileDirectory)));
        }

        return builder;
    }

    public static IHostApplicationBuilder AddBlobStorage(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(builder.Configuration.GetRequiredValue("StorageConnectionString"));
            });
        }

        return builder;
    }
}
