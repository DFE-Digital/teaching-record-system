namespace TeachingRecordSystem.SupportUi.Pages.Users;

public class UserRoleViewModel
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required IEnumerable<UserPermissionViewModel> Permissions { get; init; }
}
