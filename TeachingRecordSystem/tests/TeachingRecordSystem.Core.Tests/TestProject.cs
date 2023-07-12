using Fixie;

namespace TeachingRecordSystem.Core.Tests;

public class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        configuration.AddTrsTestFrameworkConventions(environment);
    }
}
