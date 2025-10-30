using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace TeachingRecordSystem.SupportUi.Tests;

public class RoleNamesData(bool includeNoRoles = false, params string[] except) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        var allRoles = UserRoles.All;
        IEnumerable<string?> roles = allRoles.Except(except);

        if (includeNoRoles)
        {
            roles = roles.Append(null);
        }

        return new(roles.Select(r => new TheoryDataRow(r)).ToArray());
    }

    public override bool SupportsDiscoveryEnumeration() => true;
}
