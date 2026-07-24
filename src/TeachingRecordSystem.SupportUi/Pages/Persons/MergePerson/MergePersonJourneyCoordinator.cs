using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[JourneyCoordinator(JourneyNames.MergePerson, routeValueKeys: ["personId"])]
public class MergePersonJourneyCoordinator(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<MergePersonState>
{
    // This folder doesn't set up the CurrentPersonFeature, so take the person from the journey's
    // own route values.
    private Guid PersonId => Guid.Parse(InstanceId.RouteValues["personId"]!.ToString()!);

    public override async Task<MergePersonState> GetStartingStateAsync()
    {
        var personId = PersonId;
        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == personId);

        return new MergePersonState
        {
            PersonAId = personId,
            PersonATrn = person.Trn
        };
    }

    /// <summary>
    /// Discards the journey along with any evidence file uploaded during it and returns the URL to
    /// send the user back to.
    /// </summary>
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.PersonDetail.Index(PersonId);
    }

    public IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
        PersonDetails recordToMatchAgainst,
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? emailAddress,
        string? nationalInsuranceNumber,
        Gender? gender)
    {
        return Impl().AsReadOnly();

        IEnumerable<PersonMatchedAttribute> Impl()
        {
            if (firstName == recordToMatchAgainst.FirstName)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (middleName == recordToMatchAgainst.MiddleName)
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (lastName == recordToMatchAgainst.LastName)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (dateOfBirth == recordToMatchAgainst.DateOfBirth)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (emailAddress == recordToMatchAgainst.EmailAddress)
            {
                yield return PersonMatchedAttribute.EmailAddress;
            }

            if (nationalInsuranceNumber == recordToMatchAgainst.NationalInsuranceNumber)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }

            if (gender == recordToMatchAgainst.Gender)
            {
                yield return PersonMatchedAttribute.Gender;
            }
        }
    }

    public async Task<IReadOnlyList<PotentialDuplicate>> GetPotentialDuplicatesAsync(params Guid[] personIds)
    {
        var potentialDuplicates = (await dbContext.Persons
            .IgnoreQueryFilters()
            .Where(p => personIds.Contains(p.PersonId))
            .Select(p => new PotentialDuplicate
            {
                Trn = p.Trn,
                PersonId = p.PersonId,
                Identifier = 'X', // We'll fix this below, can't do it over an IQueryable
                MatchedAttributes = Array.Empty<PersonMatchedAttribute>(),  // ditto
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                EmailAddress = p.EmailAddress,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                Gender = p.Gender,
                Status = p.Status,
                InductionStatus = p.InductionStatus,
                ActiveAlertCount = p.Alerts!.Count(a => a.IsOpen && a.DeletedOn == null),
                Attributes = new PersonDetails
                {
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    EmailAddress = p.EmailAddress,
                    NationalInsuranceNumber = p.NationalInsuranceNumber,
                    Gender = p.Gender
                }
            })
            .ToArrayAsync())
            .OrderBy(p => Array.IndexOf(personIds, p.PersonId))
            .ToArray();

        return potentialDuplicates
            .Select((r, i) => r with
            {
                Identifier = (char)('A' + i),
                MatchedAttributes = i == 0
                    ? [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ]
                    : GetPersonAttributeMatches(
                        potentialDuplicates[0].Attributes,
                        r.FirstName,
                        r.MiddleName,
                        r.LastName,
                        r.DateOfBirth,
                        r.EmailAddress,
                        r.NationalInsuranceNumber,
                        r.Gender)
            })
            .ToArray();
    }
}
