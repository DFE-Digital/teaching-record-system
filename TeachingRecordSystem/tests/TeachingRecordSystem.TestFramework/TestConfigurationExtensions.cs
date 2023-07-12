using Fixie;

namespace TeachingRecordSystem.TestFramework;

public static class TestConfigurationExtensions
{
    public static void AddTrsTestFrameworkConventions(this TestConfiguration testConfiguration, TestEnvironment environment)
    {
        var discovery = new Discovery();
        var execution = new Execution(environment);

        testConfiguration.Conventions.Add(discovery, execution);
    }
}
