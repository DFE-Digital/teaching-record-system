namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string LegacyUsers() => GetRequiredPathByPage("/LegacyUsers/Index");

    public string LegacyAddUser() => GetRequiredPathByPage("/LegacyUsers/AddUser/Index");

    public string LegacyAddUserConfirm(string userId) => GetRequiredPathByPage("/LegacyUsers/AddUser/Confirm", routeValues: new { userId });

    public string LegacyEditUser(Guid userId) => GetRequiredPathByPage("/LegacyUsers/EditUser", routeValues: new { userId });
}
