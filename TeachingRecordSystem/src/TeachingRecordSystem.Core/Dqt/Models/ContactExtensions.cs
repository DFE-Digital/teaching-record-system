using System.Text;

namespace TeachingRecordSystem.Core.Dqt.Models;

public static class ContactExtensions
{
    public static string ResolveFirstName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedFirstName : contact.FirstName) ?? string.Empty;

    public static string ResolveMiddleName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedMiddleName : contact.MiddleName) ?? string.Empty;

    public static string ResolveLastName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedLastName : contact.LastName) ?? string.Empty;

    public static string ResolveFullName(this Contact contact, bool includeMiddleName = true)
    {
        var fullName = new StringBuilder(contact.ResolveFirstName());
        if (includeMiddleName)
        {
            var middleName = contact.ResolveMiddleName();
            if (!string.IsNullOrEmpty(middleName))
            {
                if (fullName.Length > 0)
                {
                    fullName.Append(' ');
                }

                fullName.Append(middleName);
            }
        }

        var lastName = contact.ResolveLastName();
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

    public static bool HasStatedNames(this Contact contact) =>
        !string.IsNullOrEmpty(contact.dfeta_StatedFirstName) ||
        !string.IsNullOrEmpty(contact.dfeta_StatedLastName);
}
