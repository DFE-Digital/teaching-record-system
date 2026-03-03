using TeachingRecordSystem.TestCommon.Extensions;

namespace TeachingRecordSystem.TestCommon;

public class RetryOnCiFactAttribute : RetryFactAttribute
{
    public RetryOnCiFactAttribute()
    {
        MaxRetries = Environment.GetEnvironmentVariable("CI") is "true" ? 3 : 1;
    }
}
