using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core;

public static class UserRoles
{
    [Display(Name = "Administrator")]
    public const string Administrator = "Administrator";

    [Display(Name = "Helpdesk")]
    public const string Helpdesk = "Helpdesk";

    [Display(Name = "Alerts - read & write")]
    public const string AlertsReadWrite = "AlertsReadWrite";

    [Display(Name = "DBS alerts - read only")]
    public const string DbsAlertsReadOnly = "DbsAlertsReadOnly";

    [Display(Name = "DBS alerts - read & write")]
    public const string DbsAlertsReadWrite = "DbsAlertsReadWrite";

    [Display(Name = "Induction - read & write")]
    public const string InductionReadWrite = "InductionReadWrite";

    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        Administrator,
        Helpdesk,
        AlertsReadWrite,
        DbsAlertsReadOnly,
        DbsAlertsReadWrite,
        InductionReadWrite
    };

    public static string GetDisplayNameForRole(string role)
    {
        var member = typeof(UserRoles).GetField(role) ?? throw new ArgumentException("Invalid role.", nameof(role));
        return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? member.Name;
    }
}
