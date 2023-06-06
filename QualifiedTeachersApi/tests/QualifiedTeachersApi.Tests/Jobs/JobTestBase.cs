using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[Collection("Job")]
public abstract class JobTestBase : IAsyncLifetime
{
    public JobTestBase(JobFixture jobFixture)
    {
        JobFixture = jobFixture;
        JobFixture.ResetMocks();
    }

    public JobFixture JobFixture { get; }

    public Task InitializeAsync() => JobFixture.ClearData();

    public Task DisposeAsync() => Task.CompletedTask;
}
