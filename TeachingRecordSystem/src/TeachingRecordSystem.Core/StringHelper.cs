using System.Text;

namespace TeachingRecordSystem.Core;

public static class StringHelper
{
    public static string BuildFullName(string? firstName = null, string? middleName = null, string? lastName = null)
    {
        var fullName = new StringBuilder(firstName ?? "");

        if (!string.IsNullOrEmpty(middleName))
        {
            if (fullName.Length > 0)
            {
                fullName.Append(' ');
            }

            fullName.Append(middleName);
        }

        if (!string.IsNullOrEmpty(lastName))
        {
            if (fullName.Length > 0)
            {
                fullName.Append(' ');
            }

            fullName.Append(lastName);
        }

        return fullName.ToString();
    }
}
