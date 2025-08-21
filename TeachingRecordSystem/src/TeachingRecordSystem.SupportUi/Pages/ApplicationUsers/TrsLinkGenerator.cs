namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string ApplicationUsers() => GetRequiredPathByPage("/ApplicationUsers/Index");

    public string AddApplicationUser() => GetRequiredPathByPage("/ApplicationUsers/AddApplicationUser");

    public string EditApplicationUser(Guid userId) => GetRequiredPathByPage("/ApplicationUsers/EditApplicationUser", routeValues: new { userId });
}
