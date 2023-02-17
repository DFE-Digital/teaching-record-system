using Microsoft.AspNetCore.Hosting;

namespace QualifiedTeachersApi
{
    public static class WebHostEnvironmentExtensions
    {
        public static bool IsEndToEndTests(this IWebHostEnvironment environment) =>
            environment.EnvironmentName.Equals("EndToEndTests");

        public static bool IsUnitTests(this IWebHostEnvironment environment) =>
            environment.EnvironmentName.Equals("Testing");
    }
}
