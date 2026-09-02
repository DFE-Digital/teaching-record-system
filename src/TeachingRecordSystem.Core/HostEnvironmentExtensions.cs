using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core;

public static class HostEnvironmentExtensions
{
    extension(IHostEnvironment environment)
    {
        public bool IsEndToEndTests() =>
            environment.IsEnvironment("EndToEndTests");

        public bool IsTests() =>
            environment.IsEnvironment("Tests");
    }
}
