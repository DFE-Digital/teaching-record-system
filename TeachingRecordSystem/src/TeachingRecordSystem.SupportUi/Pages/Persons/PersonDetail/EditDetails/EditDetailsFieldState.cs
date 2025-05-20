namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public record EditDetailsFieldState<T>(string? Raw, T? Parsed) where T : class, IParsable<T>
{
    public static EditDetailsFieldState<T> FromRawValue(string? rawValue)
    {
        return new(rawValue, T.TryParse(rawValue, null, out var parsed) ? (T?)parsed : null);
    }
}
