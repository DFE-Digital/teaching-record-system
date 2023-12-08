using System.Reflection;
using Xunit.Sdk;

namespace TeachingRecordSystem.Api.Tests.Attributes;

public class RoleNamesData : DataAttribute
{
    private string[] RolesToExclude { get; }

    public RoleNamesData(params string[] except)
    {
        RolesToExclude = except;
    }
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var allRoles = new object[] { ApiRoles.UpdateNpq, ApiRoles.UpdatePerson, ApiRoles.GetPerson, ApiRoles.UnlockPerson };
        var excluded = allRoles.Except(RolesToExclude);
        return new[] { new object[] { excluded } };
    }
}
