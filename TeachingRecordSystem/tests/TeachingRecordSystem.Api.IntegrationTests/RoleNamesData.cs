using System.Reflection;
using Xunit.Sdk;

namespace TeachingRecordSystem.Api.IntegrationTests;

public class RoleNamesData(params string[] except) : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var allRoles = new string[] { ApiRoles.UpdateNpq, ApiRoles.UpdatePerson, ApiRoles.GetPerson, ApiRoles.UnlockPerson, ApiRoles.AssignQtls };
        var roles = allRoles.Except(except);
        return roles.Select(r => new object[] { new string[] { r } });
    }
}
