using Fixie;

namespace TeachingRecordSystem.Api.Tests;

public class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        configuration.AddTrsTestFrameworkConventions(environment);
    }
}
