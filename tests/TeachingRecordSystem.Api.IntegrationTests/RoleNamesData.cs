using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace TeachingRecordSystem.Api.IntegrationTests;

public class RoleNamesData(params string[] except) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        var roles = ApiRoles.All.Except(except);
        return new(roles.Select(r => new TheoryDataRow<string[]>([r])).ToArray());
    }

    public override bool SupportsDiscoveryEnumeration() => true;
}
