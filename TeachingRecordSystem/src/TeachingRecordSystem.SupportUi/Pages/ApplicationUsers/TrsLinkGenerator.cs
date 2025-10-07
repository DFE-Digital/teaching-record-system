namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string ApplicationUsers() => GetRequiredPathByPage("/ApplicationUsers/Index");

    public string AddApplicationUser() => GetRequiredPathByPage("/ApplicationUsers/AddApplicationUser/Index");

    public string EditApplicationUser(Guid userId) => GetRequiredPathByPage("/ApplicationUsers/EditApplicationUser/Index", routeValues: new { userId });
}
