using System.Reflection;
using Xunit.Sdk;

namespace TeachingRecordSystem.SupportUi.Tests;

public class RoleNamesData(bool includeNoRoles = false, params string[] except) : DataAttribute
{
    public override IEnumerable<object?[]> GetData(MethodInfo testMethod)
    {
        var allRoles = UserRoles.All;
        IEnumerable<string?> roles = allRoles.Except(except);

        if (includeNoRoles)
        {
            roles = roles.Append(null);
        }

        return roles.Select(r => new object?[] { r });
    }
}
