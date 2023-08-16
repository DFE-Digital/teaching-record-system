using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core;

public static class HostEnvironmentExtensions
{
    public static bool IsEndToEndTests(this IHostEnvironment environment) =>
        environment.EnvironmentName.Equals("EndToEndTests");

    public static bool IsUnitTests(this IHostEnvironment environment) =>
        environment.EnvironmentName.Equals("Testing");
}
