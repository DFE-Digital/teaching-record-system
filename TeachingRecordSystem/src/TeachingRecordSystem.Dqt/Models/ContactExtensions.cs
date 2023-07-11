namespace TeachingRecordSystem.Dqt.Models;

public static class ContactExtensions
{
    public static string ResolveFirstName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedFirstName : contact.FirstName) ?? throw new Exception("Contact is missing first name.");

    public static string ResolveMiddleName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedMiddleName : contact.MiddleName) ?? string.Empty;

    public static string ResolveLastName(this Contact contact) =>
        (contact.HasStatedNames() ? contact.dfeta_StatedLastName : contact.LastName) ?? throw new Exception("Contact is missing last name.");

    private static bool HasStatedNames(this Contact contact) =>
        !string.IsNullOrEmpty(contact.dfeta_StatedFirstName) &&
        !string.IsNullOrEmpty(contact.dfeta_StatedLastName);
}
