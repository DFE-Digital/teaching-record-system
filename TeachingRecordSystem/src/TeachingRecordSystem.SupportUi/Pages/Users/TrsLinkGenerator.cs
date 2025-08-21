namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string Users(string? keywords = null, string? status = null, string? role = null, int? pageNumber = null) =>
    GetRequiredPathByPage("/Users/Index", routeValues: new { keywords, status, role, pageNumber });

    public string AddUser() => GetRequiredPathByPage("/Users/AddUser/Index");

    public string AddUserConfirm(string userId) => GetRequiredPathByPage("/Users/AddUser/Confirm", routeValues: new { userId });

    public string EditUser(Guid userId) => GetRequiredPathByPage("/Users/EditUser/Index", routeValues: new { userId });

    public string EditUserDeactivate(Guid userId) => GetRequiredPathByPage("/Users/EditUser/Deactivate", routeValues: new { userId });

    public string EditUserDeactivateCancel(Guid userId) => GetRequiredPathByPage("/Users/EditUser/Deactivate", "cancel", routeValues: new { userId });
}
