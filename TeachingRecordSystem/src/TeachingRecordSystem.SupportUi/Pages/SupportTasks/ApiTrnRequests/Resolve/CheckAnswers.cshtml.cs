using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance]
public class CheckAnswers(
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    SupportTaskService supportTaskService,
    SupportUiLinkGenerator linkGenerator,
    IClock clock,
    PersonChangeableAttributesService changedService) :
    ResolveApiTrnRequestPageModel(dbContext)
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
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        ApiTrnRequestDataPersonAttributes? selectedPersonAttributes;

        var processContext = new ProcessContext(ProcessType.ApiTrnRequestResolving, clock.UtcNow, User.GetUserId());

        if (CreatingNewRecord)
        {
            await trnRequestService.CompleteTrnRequestWithNewRecordAsync(requestData, processContext);

            selectedPersonAttributes = null;
        }
        else
        {
            Debug.Assert(state.PersonId is not null);
            var existingPersonId = state.PersonId!.Value;
            var selectedPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == existingPersonId);

            selectedPersonAttributes = GetPersonAttributes(selectedPerson);
            var attributesToUpdate = GetAttributesToUpdate();

            await trnRequestService.UpdatePersonFromTrnRequestAsync(selectedPerson, requestData, attributesToUpdate, processContext);

            await trnRequestService.CompleteTrnRequestWithMatchedPersonAsync(
                requestData,
                (selectedPerson.PersonId, selectedPerson.Trn!),
                processContext);
        }

        Debug.Assert(requestData.ResolvedPersonId is not null);

        var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

        await supportTaskService.UpdateSupportTaskAsync<ApiTrnRequestData>(
            new()
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    ResolvedAttributes = resolvedPersonAttributes,
                    SelectedPersonAttributes = selectedPersonAttributes
                },
                Status = SupportTaskStatus.Closed,
                Comments = state.Comments
            },
            processContext);

        TempData.SetFlashSuccess(
            $"{(CreatingNewRecord ? "Record created" : "Records merged")} for {StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName)}",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(requestData.ResolvedPersonId!.Value)));

        return Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;
        ResolvableAttributes = changedService.GetResolvableAttributes(
             new List<ResolvedAttribute>
             {
                new ResolvedAttribute(PersonMatchedAttribute.Gender, state.GenderSource),
                new ResolvedAttribute(PersonMatchedAttribute.FirstName, state.FirstNameSource),
                new ResolvedAttribute(PersonMatchedAttribute.MiddleName, state.MiddleNameSource),
                new ResolvedAttribute(PersonMatchedAttribute.LastName, state.LastNameSource),
                new ResolvedAttribute(PersonMatchedAttribute.DateOfBirth, state.DateOfBirthSource),
                new ResolvedAttribute(PersonMatchedAttribute.NationalInsuranceNumber, state.NationalInsuranceNumberSource),
                new ResolvedAttribute(PersonMatchedAttribute.EmailAddress, state.EmailAddressSource)
             });

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Resolve.Merge(SupportTaskReference!, JourneyInstance!.InstanceId));
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
            FirstName = state.FirstNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.FirstName : requestData.FirstName;
            MiddleName = state.MiddleNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.MiddleName : requestData.MiddleName;
            LastName = state.LastNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.LastName : requestData.LastName;
            DateOfBirth = state.DateOfBirthSource == PersonAttributeSource.ExistingRecord ? selectedPerson.DateOfBirth : requestData.DateOfBirth;
            EmailAddress = state.EmailAddressSource == PersonAttributeSource.ExistingRecord ? selectedPerson.EmailAddress : requestData.EmailAddress;
            NationalInsuranceNumber = state.NationalInsuranceNumberSource == PersonAttributeSource.ExistingRecord ? selectedPerson.NationalInsuranceNumber : requestData.NationalInsuranceNumber;
            Gender = state.GenderSource == PersonAttributeSource.ExistingRecord ? selectedPerson.Gender : requestData.Gender;
            Trn = selectedPerson.Trn;
        }

        Comments = state.Comments;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
