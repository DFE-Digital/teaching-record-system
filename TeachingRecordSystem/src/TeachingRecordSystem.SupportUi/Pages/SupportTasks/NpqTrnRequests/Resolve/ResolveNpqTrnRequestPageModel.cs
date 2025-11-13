using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

public abstract class ResolveNpqTrnRequestPageModel(TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<ResolveNpqTrnRequestState>? JourneyInstance { get; set; }
    protected TrsDbContext DbContext => dbContext;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    protected TrnRequestMetadata GetRequestData()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask.TrnRequestMetadata!;
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        base.OnPageHandlerExecuting(context);
    }

    protected async Task<NpqTrnRequestDataPersonAttributes> GetPersonAttributesAsync(Guid personId)
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
                p.Gender
            })
            .SingleAsync();

        return new NpqTrnRequestDataPersonAttributes()
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

    protected NpqTrnRequestDataPersonAttributes GetPersonAttributesFromRequestData()
    {
        var requestData = GetRequestData();

        return new NpqTrnRequestDataPersonAttributes()
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

    protected IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
        string firstName,
        string? middleName,
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

    protected NpqTrnRequestDataPersonAttributes GetResolvedPersonAttributes(
        NpqTrnRequestDataPersonAttributes? selectedPersonAttributes)
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

            return new NpqTrnRequestDataPersonAttributes()
            {
                FirstName = selectedPersonAttributes.FirstName,
                MiddleName = selectedPersonAttributes.MiddleName,
                LastName = selectedPersonAttributes.LastName,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.DateOfBirth : trnRequestPersonAttributes.DateOfBirth,
                EmailAddress = state.EmailAddressSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.EmailAddress : trnRequestPersonAttributes.EmailAddress,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.NationalInsuranceNumber : trnRequestPersonAttributes.NationalInsuranceNumber,
                Gender = state.GenderSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.Gender : trnRequestPersonAttributes.Gender
            };
        }
    }
}
