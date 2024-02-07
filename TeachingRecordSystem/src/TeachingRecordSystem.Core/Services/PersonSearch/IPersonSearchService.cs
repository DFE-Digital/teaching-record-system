namespace TeachingRecordSystem.Core.Services.PersonSearch;

public interface IPersonSearchService
{
    Task<IReadOnlyCollection<PersonSearchResult>> Search(
        IEnumerable<string[]> names,
        IEnumerable<DateOnly> datesOfBirth,
        string? nationalInsuranceNumber,
        string? trn);
}
