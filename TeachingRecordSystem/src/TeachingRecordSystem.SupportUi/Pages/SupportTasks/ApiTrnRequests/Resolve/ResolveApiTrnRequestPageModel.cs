using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

public abstract class ResolveApiTrnRequestPageModel(TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<ResolveApiTrnRequestState>? JourneyInstance { get; set; }

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
        string? emailAddress,
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

            if (middleName == requestData.MiddleName)
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

            if (emailAddress == requestData.EmailAddress)
            {
                yield return PersonMatchedAttribute.EmailAddress;
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

    protected async Task<ApiTrnRequestDataPersonAttributes> GetPersonAttributesAsync(Guid personId)
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

        return new ApiTrnRequestDataPersonAttributes()
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

    protected ApiTrnRequestDataPersonAttributes GetPersonAttributesFromRequestData()
    {
        var requestData = GetRequestData();

        return new ApiTrnRequestDataPersonAttributes()
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

    protected ApiTrnRequestDataPersonAttributes GetResolvedPersonAttributes(
        ApiTrnRequestDataPersonAttributes? selectedPersonAttributes)
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

            return new ApiTrnRequestDataPersonAttributes()
            {
                FirstName = state.FirstNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.FirstName : trnRequestPersonAttributes.FirstName,
                MiddleName = state.MiddleNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.MiddleName : trnRequestPersonAttributes.MiddleName,
                LastName = state.LastNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.LastName : trnRequestPersonAttributes.LastName,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.DateOfBirth : trnRequestPersonAttributes.DateOfBirth,
                EmailAddress = state.EmailAddressSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.EmailAddress : trnRequestPersonAttributes.EmailAddress,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.NationalInsuranceNumber : trnRequestPersonAttributes.NationalInsuranceNumber,
                Gender = state.GenderSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.Gender : trnRequestPersonAttributes.Gender
            };
        }
    }
}

