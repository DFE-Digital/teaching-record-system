using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

public abstract class ResolveNpqTrnRequestPageModel(TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<ResolveNpqTrnRequestState>? JourneyInstance { get; set; }
    protected TrsDbContext DbContext => dbContext;

    //public SupportTask SupportTask { get; set; } // CML TODO - need this? or use the getRequestData method below when needed?

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    protected TrnRequestMetadata GetRequestData() // CML TODO decide whether to use this or Supportask above
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask.TrnRequestMetadata!;
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var supportTaskFeature = context.HttpContext.GetCurrentSupportTaskFeature();
        if (supportTaskFeature.SupportTask is not { SupportTaskType: SupportTaskType.NpqTrnRequest }) // CML TODO - dealt with by the filter?
        {
            context.Result = NotFound();
            return;
        }

        //SupportTask = supportTaskFeature.SupportTask;
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
                p.EmailAddress
            })
            .SingleAsync();

        return new NpqTrnRequestDataPersonAttributes()
        {
            FirstName = personAttributes.FirstName,
            MiddleName = personAttributes.MiddleName ?? string.Empty,
            LastName = personAttributes.LastName,
            DateOfBirth = personAttributes.DateOfBirth,
            EmailAddress = personAttributes.EmailAddress,
            NationalInsuranceNumber = personAttributes.NationalInsuranceNumber
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
            NationalInsuranceNumber = requestData.NationalInsuranceNumber
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
        }
    }

    protected IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
    string firstName,
    string middleName,
    string lastName,
    DateOnly? dateOfBirth,
    string? emailAddress,
    string? nationalInsuranceNumber)
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
                FirstName = state.FirstNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.FirstName : trnRequestPersonAttributes.FirstName,
                MiddleName = state.MiddleNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.MiddleName : trnRequestPersonAttributes.MiddleName,
                LastName = state.LastNameSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.LastName : trnRequestPersonAttributes.LastName,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.DateOfBirth : trnRequestPersonAttributes.DateOfBirth,
                EmailAddress = state.EmailAddressSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.EmailAddress : trnRequestPersonAttributes.EmailAddress,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.ExistingRecord ? selectedPersonAttributes.NationalInsuranceNumber : trnRequestPersonAttributes.NationalInsuranceNumber
            };
        }
    }
}
