namespace TeachingRecordSystem.AuthorizeAccess.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<(Guid PersonId, string Trn)?> Match(
        IEnumerable<string[]> names,
        IEnumerable<DateOnly> datesOfBirth,
        string? nationalInsuranceNumber,
        string? trn);
}
