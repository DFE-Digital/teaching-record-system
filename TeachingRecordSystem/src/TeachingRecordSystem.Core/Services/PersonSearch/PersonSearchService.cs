using LinqKit;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.PersonSearch;

public class PersonSearchService(TrsDbContext dbContext) : IPersonSearchService
{
    public async Task<IReadOnlyCollection<PersonSearchResult>> Search(
        IEnumerable<string[]> names,
        IEnumerable<DateOnly> datesOfBirth,
        string? nationalInsuranceNumber,
        string? trn)
    {
        var fullNames = names.Select(n => (FirstName: n.First(), LastName: n.Where(n => n.Length > 1).LastOrDefault())).Where(n => n.LastName is not null).Select(n => $"{n.FirstName} {n.LastName}").ToList();
        if (!fullNames.Any() || !datesOfBirth.Any() || (nationalInsuranceNumber is null && trn is null))
        {
            return Array.Empty<PersonSearchResult>();
        }

        var searchResults = new List<PersonSearchResult>();
        var dateOfBirthList = datesOfBirth.Select(d => d.ToString("yyyy-MM-dd")).ToList();

        var searchPredicate = PredicateBuilder.New<PersonSearchAttribute>(false);
        foreach (var fullName in fullNames)
        {
            searchPredicate = searchPredicate.Or(a => a.AttributeType == "FullName" && a.AttributeValue == fullName);
        }

        searchPredicate = searchPredicate.Or(a => a.AttributeType == "DateOfBirth" && dateOfBirthList.Contains(a.AttributeValue));

        if (trn is not null)
        {
            searchPredicate = searchPredicate.Or(a => a.AttributeType == "Trn" && a.AttributeValue == trn);
        }

        if (nationalInsuranceNumber is not null)
        {
            searchPredicate = searchPredicate.Or(a => a.AttributeType == "NationalInsuranceNumber" && a.AttributeValue == nationalInsuranceNumber);
        }

        var results = await dbContext.PersonSearchAttributes
            .Where(searchPredicate)
            .GroupBy(a => a.PersonId)
            .Where(g => g.Any(a => a.AttributeType == "FullName") && g.Any(a => a.AttributeType == "DateOfBirth") && (g.Any(a => a.AttributeType == "NationalInsuranceNumber") || g.Any(a => a.AttributeType == "Trn")))
            .Select(g => dbContext.Persons.Where(p => p.PersonId == g.Key).ToList())
            .ToListAsync();
        var persons = results.SelectMany(p => p).ToArray();

        return persons.Select(p => new PersonSearchResult
        {
            PersonId = p.PersonId,
            FirstName = p.FirstName,
            MiddleName = p.MiddleName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Trn = p.Trn,
            NationalInsuranceNumber = p.NationalInsuranceNumber
        }).AsReadOnly();
    }
}
