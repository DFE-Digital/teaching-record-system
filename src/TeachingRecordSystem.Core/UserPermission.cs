using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Core;

public record UserPermission(string Type, UserPermissionLevel Level)
{
    public override string ToString()
        => $"{Type}:{Level}";

    public static bool TryParse(string value, [NotNullWhen(true)] out UserPermission? result)
    {
        result = null;
        var parts = value.Split(':');
        if (parts.Length != 2
            || !UserPermissionTypes.All.Contains(parts[0])
            || !Enum.TryParse<UserPermissionLevel>(parts[1], out var level))
        {
            return false;
        }

        result = new(parts[0], level);
        return true;
    }
}
