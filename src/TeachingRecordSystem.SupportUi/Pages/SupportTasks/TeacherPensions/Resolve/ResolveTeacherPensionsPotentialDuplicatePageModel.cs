using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public abstract class ResolveTeacherPensionsPotentialDuplicatePageModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext => dbContext!;

    protected SupportTask GetSupportTask()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask;
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

    protected TeacherPensionsPotentialDuplicateAttributes GetPersonAttributes(Person person) =>
        new()
        {
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            Trn = person.Trn
        };

    protected async Task<TeacherPensionsPotentialDuplicateAttributes> GetPersonAttributesAsync(Guid personId)
    {
        var personAttributes = await DbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new
            {
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.DateOfBirth,
                p.NationalInsuranceNumber,
                p.EmailAddress,
                p.Gender,
                p.Trn
            })
            .SingleAsync();

        return new TeacherPensionsPotentialDuplicateAttributes()
        {
            FirstName = personAttributes.FirstName,
            MiddleName = personAttributes.MiddleName,
            LastName = personAttributes.LastName,
            DateOfBirth = personAttributes.DateOfBirth,
            NationalInsuranceNumber = personAttributes.NationalInsuranceNumber,
            Gender = personAttributes.Gender,
            Trn = personAttributes.Trn
        };
    }

    protected TeacherPensionsPotentialDuplicateAttributes GetPersonAttributesFromRequestData()
    {
        var supportTask = GetSupportTask();
        var person = DbContext.Persons.Single(x => x.PersonId == supportTask.PersonId);
        var requestData = supportTask.TrnRequestMetadata!;

        return new TeacherPensionsPotentialDuplicateAttributes()
        {
            FirstName = requestData.FirstName!,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName!,
            DateOfBirth = requestData.DateOfBirth,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber,
            Gender = requestData.Gender,
            Trn = person.Trn
        };
    }

    protected TeacherPensionsPotentialDuplicateAttributes GetResolvedPersonAttributes(
        TeacherPensionsPotentialDuplicateAttributes? selectedPersonAttributes)
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

            // Only a TrnRequest source updates the person (see GetAttributesToUpdate), so anything else —
            // including MiddleName, which this journey never offers a choice for — keeps the existing value.
            return new TeacherPensionsPotentialDuplicateAttributes()
            {
                FirstName = state.FirstNameSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.FirstName : selectedPersonAttributes.FirstName,
                MiddleName = state.MiddleNameSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.MiddleName : selectedPersonAttributes.MiddleName,
                LastName = state.LastNameSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.LastName : selectedPersonAttributes.LastName,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.DateOfBirth : selectedPersonAttributes.DateOfBirth,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.NationalInsuranceNumber : selectedPersonAttributes.NationalInsuranceNumber,
                Gender = state.GenderSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.Gender : selectedPersonAttributes.Gender,
                Trn = selectedPersonAttributes.Trn
            };
        }
    }
}

