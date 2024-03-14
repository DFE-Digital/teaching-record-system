using System.Reflection;
using Xunit.Sdk;

namespace TeachingRecordSystem.Api.Tests;

public class RoleNamesData(params string[] except) : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var allRoles = new object[] { ApiRoles.UpdateNpq, ApiRoles.UpdatePerson, ApiRoles.GetPerson, ApiRoles.UnlockPerson, ApiRoles.AssignQtls };
        var excluded = allRoles.Except(except);
        return new[] { new object[] { excluded } };
    }
}
