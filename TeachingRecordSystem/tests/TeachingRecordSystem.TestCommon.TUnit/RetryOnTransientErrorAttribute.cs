namespace TeachingRecordSystem.TestCommon;

public class RetryOnTransientErrorAttribute(int times) : RetryAttribute(times)
{
    public override Task<bool> ShouldRetry(TestContext context, Exception exception, int currentRetryCount)
    {
        if (exception is not InvalidOperationException { Message: "An exception has been raised that is likely due to a transient failure." })
        {
            return Task.FromResult(false);
        }

        return base.ShouldRetry(context, exception, currentRetryCount);
    }
}
