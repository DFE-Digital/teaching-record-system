using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    SupportTaskService supportTaskService,
    SupportUiLinkGenerator linkGenerator,
    IClock clock,
    PersonChangeableAttributesService changedService) : ResolveNpqTrnRequestPageModel(dbContext)
{
    public string? SourceApplicationUserName { get; set; }

    public bool CreatingNewRecord { get; set; }

    public bool? PotentialDuplicate { get; set; }

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

        NpqTrnRequestDataPersonAttributes? selectedPersonAttributes;

        var processContext = new ProcessContext(ProcessType.NpqTrnRequestApproving, clock.UtcNow, User.GetUserId());

        if (CreatingNewRecord)
        {
            await trnRequestService.CompleteTrnRequestWithNewRecordAsync(trnRequest, processContext);

            selectedPersonAttributes = null;
        }
        else // updating
        {
            Debug.Assert(state.PersonId is not null);
            var existingPersonId = state.PersonId!.Value;
            var selectedPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == existingPersonId);

            selectedPersonAttributes = await GetPersonAttributesAsync(existingPersonId);
            var attributesToUpdate = GetAttributesToUpdate();

            await trnRequestService.CompleteTrnRequestWithMatchedPersonAsync(
                trnRequest,
                selectedPerson,
                attributesToUpdate,
                processContext);
        }

        Debug.Assert(trnRequest.ResolvedPersonId is not null);

        var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<NpqTrnRequestData>
            {
                SupportTaskReference = SupportTaskReference,
                UpdateData = data => data with
                {
                    SupportRequestOutcome = SupportRequestOutcome.Approved,
                    ResolvedAttributes = resolvedPersonAttributes,
                    SelectedPersonAttributes = selectedPersonAttributes
                },
                Status = SupportTaskStatus.Closed,
                Comments = Comments,
                RejectionReason = null
            },
            processContext);

        TempData.SetFlashSuccess(
            $"TRN request for {StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName)} completed",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(trnRequest.ResolvedPersonId!.Value)));

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Resolve.Merge(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            FirstName = requestData.FirstName;
            MiddleName = requestData.MiddleName;
            LastName = requestData.LastName;
            CreatingNewRecord = true;
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
            FirstName = selectedPerson.FirstName;
            MiddleName = selectedPerson.MiddleName;
            LastName = selectedPerson.LastName;
            DateOfBirth = state.DateOfBirthSource == PersonAttributeSource.ExistingRecord ? selectedPerson.DateOfBirth : requestData.DateOfBirth;
            EmailAddress = state.EmailAddressSource == PersonAttributeSource.ExistingRecord ? selectedPerson.EmailAddress : requestData.EmailAddress;
            NationalInsuranceNumber = state.NationalInsuranceNumberSource == PersonAttributeSource.ExistingRecord ? selectedPerson.NationalInsuranceNumber : requestData.NationalInsuranceNumber;
            Gender = state.GenderSource == PersonAttributeSource.ExistingRecord ? selectedPerson.Gender : requestData.Gender;
            Trn = selectedPerson.Trn;
        }

        Comments = state.Comments;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;
        PotentialDuplicate = requestData.PotentialDuplicate;

        ResolvableAttributes = changedService.GetResolvableAttributes(
        [
            new ResolvedAttribute(PersonMatchedAttribute.Gender, state.GenderSource),
            new ResolvedAttribute(PersonMatchedAttribute.DateOfBirth, state.DateOfBirthSource),
            new ResolvedAttribute(PersonMatchedAttribute.NationalInsuranceNumber, state.NationalInsuranceNumberSource),
            new ResolvedAttribute(PersonMatchedAttribute.EmailAddress, state.EmailAddressSource)
        ]);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
