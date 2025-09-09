using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public abstract class ResolveTeacherPensionsPotentialDuplicatePageModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext => dbContext;

    protected TrnRequestMetadata GetRequestData()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask.TrnRequestMetadata!;
    }

    protected IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? nationalInsuranceNumber,
        Gender? gender)
    {
        return Impl().AsReadOnly();

        IEnumerable<PersonMatchedAttribute> Impl()
        {
            var requestData = GetRequestData();

            if (firstName == requestData.FirstName)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (middleName == requestData.MiddleName || (string.IsNullOrWhiteSpace(requestData.MiddleName) && string.IsNullOrWhiteSpace(middleName)))
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (lastName == requestData.LastName)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (dateOfBirth == requestData.DateOfBirth)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (nationalInsuranceNumber == requestData.NationalInsuranceNumber)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }

            if (gender == requestData.Gender)
            {
                yield return PersonMatchedAttribute.Gender;
            }
        }
    }

    protected IReadOnlyCollection<PersonMatchedAttribute> GetAttributesToUpdate()
    {
        var state = JourneyInstance!.State;

        if (!state.PersonAttributeSourcesSet)
        {
            throw new InvalidOperationException("Attribute sources not set.");
        }

        return Impl().AsReadOnly();

        IEnumerable<PersonMatchedAttribute> Impl()
        {
            if (state.FirstNameSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (state.MiddleNameSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (state.LastNameSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (state.DateOfBirthSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }

            if (state.GenderSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.Gender;
            }
        }
    }

    protected CapitaPotentialDuplicateAttributes GetPersonAttributes(Person person) =>
        new()
        {
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender
        };

    protected async Task<CapitaPotentialDuplicateAttributes> GetPersonAttributesAsync(Guid personId)
    {
        var personAttributes = await dbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new
            {
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.DateOfBirth,
                p.NationalInsuranceNumber,
                p.EmailAddress,
                p.Gender
            })
            .SingleAsync();

        return new CapitaPotentialDuplicateAttributes()
        {
            FirstName = personAttributes.FirstName,
            MiddleName = personAttributes.MiddleName,
            LastName = personAttributes.LastName,
            DateOfBirth = personAttributes.DateOfBirth,
            NationalInsuranceNumber = personAttributes.NationalInsuranceNumber,
            Gender = personAttributes.Gender
        };
    }

    protected CapitaPotentialDuplicateAttributes GetPersonAttributesFromRequestData()
    {
        var requestData = GetRequestData();

        return new CapitaPotentialDuplicateAttributes()
        {
            FirstName = requestData.FirstName!,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName!,
            DateOfBirth = requestData.DateOfBirth,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber,
            Gender = requestData.Gender
        };
    }

    protected CapitaPotentialDuplicateAttributes GetResolvedPersonAttributes(
        CapitaPotentialDuplicateAttributes? selectedPersonAttributes)
    {
        var state = JourneyInstance!.State;
        var trnRequestPersonAttributes = GetPersonAttributesFromRequestData();

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            Debug.Assert(selectedPersonAttributes is null);
            return trnRequestPersonAttributes;
        }
        else
        {
            Debug.Assert(selectedPersonAttributes is not null);

            return new CapitaPotentialDuplicateAttributes()
            {
                FirstName = state.FirstNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.FirstName : trnRequestPersonAttributes.FirstName,
                MiddleName = state.MiddleNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.MiddleName : trnRequestPersonAttributes.MiddleName,
                LastName = state.LastNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.LastName : trnRequestPersonAttributes.LastName,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.DateOfBirth : trnRequestPersonAttributes.DateOfBirth,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.NationalInsuranceNumber : trnRequestPersonAttributes.NationalInsuranceNumber,
                Gender = state.GenderSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.Gender : trnRequestPersonAttributes.Gender
            };
        }
    }
}

