using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve.ResolveTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[Journey(JourneyNames.ResolveTrnRequest), RequireJourneyInstance]
public class CheckAnswers(
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider,
    PersonChangeableAttributesService changedService) :
    ResolveTrnRequestPageModel(dbContext)
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public bool CreatingNewRecord { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public Gender? Gender { get; set; }

    public string? Trn { get; set; }

    public string? Comments { get; set; }

    public IEnumerable<ResolvedAttribute>? ResolvableAttributes { get; private set; }

    public bool IsGenderChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.Gender) == true;

    public bool IsFirstNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.FirstName) == true;

    public bool IsMiddleNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.MiddleName) == true;

    public bool IsLastNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.LastName) == true;

    public bool IsDateOfBirthChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.DateOfBirth) == true;

    public bool IsNationalInsuranceNumberChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.NationalInsuranceNumber) == true;

    public bool IsEmailAddressChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.EmailAddress) == true;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var trnRequest = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        var processContext = new ProcessContext(ProcessType.TrnRequestResolving, timeProvider.UtcNow, User.GetUserId());

        Guid? existingPersonId = null;

        // A new record takes every value from the request, so it has no sources to choose and never visits
        // the page that sets them.
        var attributeSources = new PersonAttributeSources();

        if (!CreatingNewRecord)
        {
            Debug.Assert(state.PersonId is not null);
            existingPersonId = state.PersonId!.Value;
            attributeSources = GetPersonAttributeSources();
        }

        var resolvedPersonId = await trnRequestService.ResolveTrnRequestAsync(
            new ResolveTrnRequestOptions
            {
                ApplicationUserId = trnRequest.ApplicationUserId,
                RequestId = trnRequest.RequestId,
                SupportTaskReference = supportTask.SupportTaskReference,
                PersonId = existingPersonId,
                AttributeSources = attributeSources,
                Comments = state.Comments
            },
            processContext);

        TempData.SetFlashNotificationBanner(
            $"{(CreatingNewRecord ? "Record created" : "Records merged")} for {string.JoinNonEmpty(' ', FirstName, MiddleName, LastName)}",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(resolvedPersonId)));

        return Redirect(linkGenerator.SupportTasks.TrnRequests.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.TrnRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TrnRequests.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TrnRequests.Resolve.Merge(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            CreatingNewRecord = true;
            FirstName = requestData.FirstName;
            MiddleName = requestData.MiddleName;
            LastName = requestData.LastName;
            DateOfBirth = requestData.DateOfBirth;
            EmailAddress = requestData.EmailAddress;
            NationalInsuranceNumber = requestData.NationalInsuranceNumber;
            Gender = requestData.Gender;
            Trn = null;
        }
        else
        {
            Debug.Assert(state.PersonId is not null);

            var selectedPerson = await DbContext.Persons
                .Where(p => p.PersonId == state.PersonId)
                .Select(p => new
                {
                    p.FirstName,
                    p.MiddleName,
                    p.LastName,
                    p.DateOfBirth,
                    p.EmailAddress,
                    p.NationalInsuranceNumber,
                    p.Gender,
                    p.Trn
                })
                .SingleAsync();

            CreatingNewRecord = false;
            // Mirrors GetResolvedPersonAttributes: only a TrnRequest source changes the record.
            FirstName = state.FirstNameSource is PersonAttributeSource.TrnRequest ? requestData.FirstName : selectedPerson.FirstName;
            MiddleName = state.MiddleNameSource is PersonAttributeSource.TrnRequest ? requestData.MiddleName : selectedPerson.MiddleName;
            LastName = state.LastNameSource is PersonAttributeSource.TrnRequest ? requestData.LastName : selectedPerson.LastName;
            DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? requestData.DateOfBirth : selectedPerson.DateOfBirth;
            EmailAddress = state.EmailAddressSource is PersonAttributeSource.TrnRequest ? requestData.EmailAddress : selectedPerson.EmailAddress;
            NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? requestData.NationalInsuranceNumber : selectedPerson.NationalInsuranceNumber;
            Gender = state.GenderSource is PersonAttributeSource.TrnRequest ? requestData.Gender : selectedPerson.Gender;
            Trn = selectedPerson.Trn;
        }

        Comments = state.Comments;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        ResolvableAttributes = changedService.GetResolvableAttributes(
        [
            new ResolvedAttribute(PersonMatchedAttribute.Gender, state.GenderSource),
            new ResolvedAttribute(PersonMatchedAttribute.FirstName, state.FirstNameSource),
            new ResolvedAttribute(PersonMatchedAttribute.MiddleName, state.MiddleNameSource),
            new ResolvedAttribute(PersonMatchedAttribute.LastName, state.LastNameSource),
            new ResolvedAttribute(PersonMatchedAttribute.DateOfBirth, state.DateOfBirthSource),
            new ResolvedAttribute(PersonMatchedAttribute.NationalInsuranceNumber, state.NationalInsuranceNumberSource),
            new ResolvedAttribute(PersonMatchedAttribute.EmailAddress, state.EmailAddressSource)
        ]);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
