using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve.ResolveTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

public abstract class ResolveTrnRequestPageModel(TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<ResolveTrnRequestState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext => dbContext;

    protected TrnRequestMetadata GetRequestData()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask.TrnRequestMetadata!;
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

            if (state.EmailAddressSource is PersonAttributeSource.TrnRequest)
            {
                yield return PersonMatchedAttribute.EmailAddress;
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

    protected TrnRequestDataPersonAttributes GetPersonAttributes(Person person) =>
        new()
        {
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = person.EmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender
        };

    protected async Task<TrnRequestDataPersonAttributes> GetPersonAttributesAsync(Guid personId)
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

        return new TrnRequestDataPersonAttributes()
        {
            FirstName = personAttributes.FirstName,
            MiddleName = personAttributes.MiddleName,
            LastName = personAttributes.LastName,
            DateOfBirth = personAttributes.DateOfBirth,
            EmailAddress = personAttributes.EmailAddress,
            NationalInsuranceNumber = personAttributes.NationalInsuranceNumber,
            Gender = personAttributes.Gender
        };
    }

    protected TrnRequestDataPersonAttributes GetPersonAttributesFromRequestData()
    {
        var requestData = GetRequestData();

        return new TrnRequestDataPersonAttributes()
        {
            FirstName = requestData.FirstName!,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName!,
            DateOfBirth = requestData.DateOfBirth,
            EmailAddress = requestData.EmailAddress,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber,
            Gender = requestData.Gender
        };
    }

    protected TrnRequestDataPersonAttributes GetResolvedPersonAttributes(
        TrnRequestDataPersonAttributes? selectedPersonAttributes)
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
            // including a source left unset because the page didn't offer a choice — keeps the existing value.
            return new TrnRequestDataPersonAttributes()
            {
                FirstName = state.FirstNameSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.FirstName : selectedPersonAttributes.FirstName,
                MiddleName = state.MiddleNameSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.MiddleName : selectedPersonAttributes.MiddleName,
                LastName = state.LastNameSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.LastName : selectedPersonAttributes.LastName,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.DateOfBirth : selectedPersonAttributes.DateOfBirth,
                EmailAddress = state.EmailAddressSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.EmailAddress : selectedPersonAttributes.EmailAddress,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.NationalInsuranceNumber : selectedPersonAttributes.NationalInsuranceNumber,
                Gender = state.GenderSource is PersonAttributeSource.TrnRequest ? trnRequestPersonAttributes.Gender : selectedPersonAttributes.Gender
            };
        }
    }
}

