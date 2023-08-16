using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core;

public static class UserRoles
{
    [Display(Name = "Administrator")]
    public const string Administrator = "Administrator";

    [Display(Name = "Helpdesk")]
    public const string Helpdesk = "Helpdesk";

    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        Administrator,
        Helpdesk
    };

    public static string GetDisplayNameForRole(string role)
    {
        var member = typeof(UserRoles).GetField(role) ?? throw new ArgumentException("Invalid role.", nameof(role));
        return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? member.Name;
    }
}
