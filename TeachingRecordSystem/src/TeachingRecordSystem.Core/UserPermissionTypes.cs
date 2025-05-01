using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core;

public static class UserPermissionTypes
{
    [Display(Name = "Person data")]
    public const string PersonData = "PersonData";

    [Display(Name = "Non-person or alert data", Description = "All data in the TRS console excluding alerts")]
    public const string NonPersonOrAlertData = "NonPersonOrAlertData";

    [Display(Name = "Non-DBS alerts")]
    public const string NonDbsAlerts = "NonDbsAlerts";

    [Display(Name = "DBS alerts")]
    public const string DbsAlerts = "DbsAlerts";

    [Display(Name = "Manage users", Description = "Add, manage, and remove other users from the TRS console")]
    public const string ManageUsers = "ManageUsers";

    [Display(Name = "Support tasks", Description = "Access to the Support tasks area of the console")]
    public const string SupportTasks = "SupportTasks";

    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        PersonData,
        NonPersonOrAlertData,
        NonDbsAlerts,
        DbsAlerts,
        ManageUsers,
        SupportTasks
    };

    public static string GetDisplayNameForPermissionType(string permissionType)
    {
        var member = typeof(UserPermissionTypes).GetField(permissionType) ?? throw new ArgumentException($@"Invalid permission type: ""{permissionType}"".", nameof(permissionType));
        return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? member.Name;
    }

    public static string GetDisplayDescriptionForPermissionType(string permissionType)
    {
        var member = typeof(UserPermissionTypes).GetField(permissionType) ?? throw new ArgumentException($@"Invalid permission type: ""{permissionType}"".", nameof(permissionType));
        return member.GetCustomAttribute<DisplayAttribute>()?.Description ?? string.Empty;
    }
}
