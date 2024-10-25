using System.Reflection;
using Xunit.Sdk;

namespace TeachingRecordSystem.SupportUi.Tests;

public class RoleNamesData(params string[] except) : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var allRoles = UserRoles.All;
        var roles = allRoles.Except(except);
        return roles.Select(r => new object[] { r });
    }
}
