using TeachingRecordSystem.TestCommon.Extensions;

namespace TeachingRecordSystem.TestCommon;

public class RetryOnCiTheoryAttribute : RetryTheoryAttribute
{
    public RetryOnCiTheoryAttribute()
    {
        MaxRetries = Environment.GetEnvironmentVariable("CI") is "true" ? 3 : 1;
    }
}
