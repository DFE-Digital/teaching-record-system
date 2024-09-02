using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.Services.WorkforceData.Google;

public class OptionsStorageClientProvider : IStorageClientProvider
{
    private readonly IOptions<WorkforceDataExportOptions> _optionsAccessor;

    public OptionsStorageClientProvider(IOptions<WorkforceDataExportOptions> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        _optionsAccessor = optionsAccessor;
    }

    public ValueTask<StorageClient> GetStorageClientAsync()
    {
        var configuredClient = _optionsAccessor.Value.StorageClient ??
            throw new InvalidOperationException($"No {nameof(WorkforceDataExportOptions.StorageClient)} has been configured.");

        return new ValueTask<StorageClient>(configuredClient);
    }
}
