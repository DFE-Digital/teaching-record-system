namespace TeachingRecordSystem.Core.Dqt.Models;

public static class ContactExtensions
{
    public static string ResolveFirstName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedFirstName : contact.FirstName) ?? string.Empty;

    public static string ResolveMiddleName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedMiddleName : contact.MiddleName) ?? string.Empty;

    public static string ResolveLastName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedLastName : contact.LastName) ?? string.Empty;

    public static string ResolveFullName(this Contact contact, bool includeMiddleName = true) =>
        StringHelper.JoinNonEmpty(' ',
            contact.ResolveFirstName(),
            includeMiddleName ? contact.ResolveMiddleName() : null,
            contact.ResolveLastName());

    public static bool HasStatedNames(this Contact contact) =>
        !string.IsNullOrEmpty(contact.dfeta_StatedFirstName) &&
        !string.IsNullOrEmpty(contact.dfeta_StatedLastName);
}
