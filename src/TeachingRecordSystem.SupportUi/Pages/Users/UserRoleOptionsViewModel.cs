namespace TeachingRecordSystem.SupportUi.Pages.Users;

public class UserRoleOptionsViewModel
{
    public required string FieldName { get; init; }
    public required IReadOnlyCollection<UserRoleViewModel> Roles { get; init; }
    public required string? SelectedRole { get; init; }
    public required bool Disabled { get; init; }
}
