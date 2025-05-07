using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance]
public class CheckAnswers(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    ITrnGenerator trnGenerator,
    TrsLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public JourneyInstance<ResolveApiTrnRequestState>? JourneyInstance { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public bool CreatingNewRecord { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public string? Trn { get; set; }

    public string? Comments { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        if (CreatingNewRecord)
        {
            var newContactId = Guid.NewGuid();
            requestData.ResolvedPersonId = newContactId;
            var trn = await trnGenerator.GenerateTrnAsync();

            await crmQueryDispatcher.ExecuteQueryAsync(new CreateContactQuery()
            {
                ContactId = newContactId,
                // These three name fields need normalizing; we'll cover that when moving this into a background job
                FirstName = requestData.FirstName!,
                MiddleName = requestData.MiddleName ?? string.Empty,
                LastName = requestData.LastName!,
                StatedFirstName = requestData.FirstName!,
                StatedMiddleName = requestData.MiddleName ?? string.Empty,
                StatedLastName = requestData.LastName!,
                DateOfBirth = requestData.DateOfBirth,
                Gender = Contact_GenderCode.Notprovided, // TODO when we've sorted gender
                EmailAddress = requestData.EmailAddress,
                NationalInsuranceNumber = requestData.NationalInsuranceNumber,
                ReviewTasks = [],
                ApplicationUserName = requestData.ApplicationUser.Name,
                Trn = trn,
                TrnRequestId = TrnRequestHelper.GetCrmTrnRequestId(requestData.ApplicationUserId, requestData.RequestId),
                TrnRequestMetadataMessage = null, // We don't need to pass this as we've always got metadata in our DB
                AllowPiiUpdates = false
            });
        }
        else
        {
            Debug.Assert(state.PersonId is not null);
            var existingContactId = state.PersonId!.Value;
            requestData.ResolvedPersonId = existingContactId;

            await crmQueryDispatcher.ExecuteQueryAsync(new UpdateContactQuery()
            {
                ContactId = existingContactId,
                // These three name fields need normalizing; we'll cover that when moving this into a background job
                FirstName = state.FirstNameSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.FirstName!) : default,
                MiddleName = state.MiddleNameSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.MiddleName ?? string.Empty) : default,
                LastName = state.LastNameSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.LastName!) : default,
                StatedFirstName = state.FirstNameSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.FirstName!) : default,
                StatedMiddleName = state.MiddleNameSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.MiddleName ?? string.Empty) : default,
                StatedLastName = state.LastNameSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.LastName!) : default,
                DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.DateOfBirth) : default,
                Gender = default, // TODO when we've sorted gender
                EmailAddress = state.EmailAddressSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.EmailAddress) : default,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? Option.Some(requestData.NationalInsuranceNumber) : default
            });
        }

        supportTask.Status = SupportTaskStatus.Closed;

        //TODO event
        //TODO stash the chosen attributes on the SupportTask's data

        await dbContext.SaveChangesAsync();

        // This is a little ugly but pushing this into a partial and executing it here is tricky
        var flashMessageHtml =
            $@"
            <a href=""{linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value)}"" class=""govuk-link"">View record</a>
            ";

        TempData.SetFlashSuccess(
            $"Records merged successfully for {FirstName} {MiddleName} {LastName}",
            messageHtml: flashMessageHtml);

        return Redirect(linkGenerator.ApiTrnRequests());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.ApiTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.ApiTrnRequestMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.ApiTrnRequestMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
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
            Trn = null;
        }
        else
        {
            var selectedPerson = await dbContext.Persons
                .Where(p => p.PersonId == state.PersonId)
                .Select(p => new
                {
                    p.FirstName,
                    p.MiddleName,
                    p.LastName,
                    p.DateOfBirth,
                    p.EmailAddress,
                    p.NationalInsuranceNumber,
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
            Trn = selectedPerson.Trn;
        }

        Comments = state.Comments;
        SourceApplicationUserName = requestData.ApplicationUser.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
