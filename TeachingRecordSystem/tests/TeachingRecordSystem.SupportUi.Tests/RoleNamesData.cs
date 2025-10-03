namespace TeachingRecordSystem.SupportUi.Tests;

public class RoleNamesData(bool includeNoRoles = false, params string[] except) : DataSourceGeneratorAttribute<string?>
{
    protected override IEnumerable<Func<string?>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var allRoles = UserRoles.All;
        IEnumerable<string?> roles = allRoles.Except(except);

        if (includeNoRoles)
        {
            roles = roles.Append(null);
        }

        return roles.Select(r => (Func<string?>)(() => r));
    }
}
