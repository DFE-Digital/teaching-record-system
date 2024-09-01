using System.Diagnostics.CodeAnalysis;
using Google.Cloud.Storage.V1;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class WorkforceDataExportOptions
{
    public StorageClient? StorageClient { get; set; }
    [DisallowNull]
    public string? BucketName { get; set; }

    [MemberNotNull(nameof(BucketName))]
    internal void ValidateOptions()
    {
        if (BucketName is null)
        {
            throw new InvalidOperationException($"{nameof(BucketName)} has not been configured.");
        }
    }
}
