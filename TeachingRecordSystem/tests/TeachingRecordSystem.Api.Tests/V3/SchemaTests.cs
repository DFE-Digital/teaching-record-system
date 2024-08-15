using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.Tests.V3;

public class SchemaTests
{
    [Fact]
    public void CheckApiVersionReferences()
    {
        // Check that each API version only references types from its own version

        var allTypes = typeof(Api.V3.Constants).Assembly.GetTypes();

        foreach (var version in VersionRegistry.AllV3MinorVersions)
        {
            var versionNamespace = $"{typeof(Api.V3.Constants).Namespace}.V{version}";
            var versionTypes = allTypes.Where(t => t.Namespace?.StartsWith(versionNamespace) == true).ToArray();

            foreach (var type in versionTypes)
            {
                var properties = type.GetProperties();

                foreach (var property in properties)
                {
                    var propertyIsVersionedType = property.PropertyType.Namespace?.StartsWith($"{typeof(Api.V3.Constants).Namespace}.V") == true;

                    if (propertyIsVersionedType && !property.PropertyType.Namespace!.StartsWith(versionNamespace))
                    {
                        Assert.Fail($"The '{property.Name}' property on {type.FullName} references a schema outside of its version ('{property.PropertyType.FullName}').");
                    }
                }

                // If the type is a controller, check the responses it's sending are the correct version.
                // The ProducesResponseTypeAttribute is the simplest way to check this.
                if (type.Name.EndsWith("Controller"))
                {
                    var actions = type.GetMethods();

                    foreach (var action in actions)
                    {
                        var producesResponseTypeAttrs = action.GetCustomAttributes<ProducesResponseTypeAttribute>();

                        foreach (var attr in producesResponseTypeAttrs)
                        {
                            if (attr.Type.Namespace?.StartsWith($"{typeof(Api.V3.Constants).Namespace}.V") == true &&
                                !attr.Type.Namespace!.StartsWith(versionNamespace))
                            {
                                Assert.Fail($"The '{action.Name}' method on {type.FullName} references a schema outside of its version ('{attr.Type.FullName}').");
                            }
                        }
                    }
                }
            }
        }
    }
}
