using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core;

public static class UserRoles
{
    [Display(Name = "Viewer")]
    public const string Viewer = nameof(Viewer);

    public static readonly IReadOnlyCollection<UserPermission> ViewerPermissions = [
        new(UserPermissionTypes.PersonData, UserPermissionLevel.View),
        new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.View),
        new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.View)
    ];

    [Display(Name = "Record manager")]
    public const string RecordManager = nameof(RecordManager);

    public static readonly IReadOnlyCollection<UserPermission> RecordManagerPermissions = [
        new(UserPermissionTypes.PersonData, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.View),
        new(UserPermissionTypes.SupportTasks, UserPermissionLevel.Edit)
    ];

    [Display(Name = "Alerts manager (TRA decisions)")]
    public const string AlertsManagerTra = nameof(AlertsManagerTra);

    public static readonly IReadOnlyCollection<UserPermission> AlertsManagerTraPermissions = [
        new(UserPermissionTypes.PersonData, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.View),
        new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.View),
        new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.Edit)
    ];

    [Display(Name = "Alerts manager (TRA and DBS decisions)")]
    public const string AlertsManagerTraDbs = nameof(AlertsManagerTraDbs);

    public static readonly IReadOnlyCollection<UserPermission> AlertsManagerTraDbsPermissions = [
        new(UserPermissionTypes.PersonData, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.View),
        new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.Edit)
    ];

    [Display(Name = "Access manager")]
    public const string AccessManager = nameof(AccessManager);

    public static readonly IReadOnlyCollection<UserPermission> AccessManagerPermissions = [
        new(UserPermissionTypes.PersonData, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.Edit),
        new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.View),
        new(UserPermissionTypes.ManageUsers, UserPermissionLevel.Edit),
        new(UserPermissionTypes.SupportTasks, UserPermissionLevel.Edit)
    ];

    [Display(Name = "Administrator")]
    public const string Administrator = nameof(Administrator);

    public static IReadOnlyCollection<string> All { get; } =
    [
        Viewer,
        RecordManager,
        AlertsManagerTra,
        AlertsManagerTraDbs,
        AccessManager,
        Administrator
    ];

    public static string GetDisplayNameForRole(string role)
    {
        var member = typeof(UserRoles).GetField(role) ?? throw new ArgumentException($@"Invalid role: ""{role}"".", nameof(role));
        return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? member.Name;
    }

    public static IEnumerable<UserPermission> GetPermissionsForRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return [];
        }

        if (role == Administrator)
        {
            return UserPermissionTypes.All.Select(p => new UserPermission(p, UserPermissionLevel.Edit));
        }

        var member = typeof(UserRoles).GetField($"{role}Permissions") ?? throw new ArgumentException($@"Invalid role: ""{role}"".", nameof(role));
        var actualPermissions = member.GetValue(null) is IEnumerable<UserPermission> rolePermissions
            ? rolePermissions
            : [];

        return UserPermissionTypes.All.Select(t => actualPermissions.FirstOrDefault(p => p.Type == t)
            ?? new UserPermission(t, UserPermissionLevel.None));
    }
}
