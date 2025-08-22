namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class RetryOnCIAttribute : RetryAttribute
{
    public RetryOnCIAttribute(int times) : base(times)
    {
    }

    public override Task<bool> ShouldRetry(TestContext context, Exception exception, int currentRetryCount)
    {
        if (Environment.GetEnvironmentVariable("CI") != "true")
        {
            return Task.FromResult(false);
        }

        return base.ShouldRetry(context, exception, currentRetryCount);
    }
}
