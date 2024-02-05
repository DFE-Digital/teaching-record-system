namespace TeachingRecordSystem.Core.Services.PersonSearch;

public interface IPersonSearchService
{
    Task<IReadOnlyCollection<PersonSearchResult>> Search(IEnumerable<string[]> name, IEnumerable<DateOnly> dateOfBirth, string? nino, string? trn);
}
