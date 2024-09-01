using Google.Cloud.Storage.V1;

namespace TeachingRecordSystem.Core.Services.WorkforceData.Google;

public interface IStorageClientProvider
{
    ValueTask<StorageClient> GetStorageClientAsync();
}
