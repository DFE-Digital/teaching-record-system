using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace TeachingRecordSystem.Api.IntegrationTests;

public class RoleNamesData(params string[] except) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        var allRoles = ApiRoles.All;
        var roles = allRoles.Except(except);
        return new ValueTask<IReadOnlyCollection<ITheoryDataRow>>(roles.Select(r => new TheoryDataRow<string[]>([r])).AsReadOnly());
    }

    public override bool SupportsDiscoveryEnumeration() => true;
}
