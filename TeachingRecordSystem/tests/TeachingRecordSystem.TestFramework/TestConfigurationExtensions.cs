using Fixie;

namespace TeachingRecordSystem.TestFramework;

public static class TestConfigurationExtensions
{
    public static void AddTrsTestFrameworkConventions(this TestConfiguration configuration, TestEnvironment environment)
    {
        var discovery = new Discovery();
        var execution = new Execution(environment);

        configuration.Conventions.Add(discovery, execution);

        if (environment.IsContinuousIntegration())
        {
            configuration.Reports.Add(new GitHubReport(environment));
        }
    }
}
