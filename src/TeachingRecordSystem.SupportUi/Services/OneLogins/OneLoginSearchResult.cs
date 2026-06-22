namespace TeachingRecordSystem.SupportUi.Services.OneLogins;

public class OneLoginSearchResult
{
    public required IReadOnlyList<OneLoginSearchResultItem> Results { get; init; }
}

public record OneLoginSearchResultItem(
    string Subject,
    string EmailAddress,
    string[][]? VerifiedNames,
    DateOnly[]? VerifiedDatesOfBirth,
    string? Trn)
{
    public string? Name => VerifiedNames != null && VerifiedNames.Length > 0 && VerifiedNames[0].Length > 0
        ? string.Join(' ', VerifiedNames[0])
        : null;

    public DateOnly? DateOfBirth => VerifiedDatesOfBirth != null && VerifiedDatesOfBirth.Length > 0
        ? VerifiedDatesOfBirth[0]
        : null;
}
