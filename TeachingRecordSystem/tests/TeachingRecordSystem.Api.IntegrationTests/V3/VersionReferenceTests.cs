using System.Reflection;
using Xunit.Sdk;

namespace TeachingRecordSystem.Api.IntegrationTests.V3;

public class VersionReferenceTests
{
    public static IEnumerable<object[]> MinorVersions => VersionRegistry.AllV3MinorVersions.Select(v => new object[] { v });

    [Theory]
    [MemberData(nameof(MinorVersions))]
    public void CheckInterVersionDependencies(string minorVersion)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name is "TeachingRecordSystem.Api");

        var typesForVersion = assemblies
            .SelectMany(GetTypesForVersion)
            .ToArray();

        var visited = new HashSet<string>();

        foreach (var type in typesForVersion)
        {
            VisitType(type);
        }

        static string? GetVersionForType(Type type)
        {
            if (type.Namespace is null)
            {
                return null;
            }

            var namespaceParts = type.Namespace.Split('.');
            return namespaceParts.FirstOrDefault(p => VersionRegistry.AllV3MinorVersions.Any(v => $"V{v}" == p))?.TrimStart('V');
        }

        IEnumerable<Type> GetTypesForVersion(Assembly assembly) =>
            assembly.GetTypes().Where(t => GetVersionForType(t) == minorVersion);

        void VisitType(Type type)
        {
            var name = type.FullName;
            if (name is null || !visited.Add(name))
            {
                return;
            }

            var typeVersion = GetVersionForType(type);
            if (typeVersion is not null && typeVersion != minorVersion && type.Namespace?.Contains("Core.ApiSchema") != true)
            {
                throw new XunitException(
                    $"Version {minorVersion} references a type in another version ({type.FullName}).");
            }

            if (type.Namespace?.Contains("Implementation.Dtos") == true)
            {
                throw new XunitException(
                    $"Version {minorVersion} references an internal implementation type ({type.FullName}).");
            }

            foreach (var property in type.GetProperties())
            {
                VisitType(property.PropertyType);
            }
        }
    }
}
