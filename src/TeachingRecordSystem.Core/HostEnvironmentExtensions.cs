using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core;

public static class HostEnvironmentExtensions
{
    public static bool IsEndToEndTests(this IHostEnvironment environment) =>
        environment.IsEnvironment("EndToEndTests");

    public static bool IsTests(this IHostEnvironment environment) =>
        environment.IsEnvironment("Tests");
}
