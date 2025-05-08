using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.WebCommon;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance, TransactionScope]
public class CheckAnswers(
    TrsDbContext dbContext,
    IBackgroundJobScheduler backgroundJobScheduler,
    TrnRequestService trnRequestService,
    ITrnGenerator trnGenerator,
    TrsLinkGenerator linkGenerator,
    IClock clock) :
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

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        ApiTrnRequestDataPersonAttributes? selectedPersonAttributes;
        EventModels.TrnRequestPersonAttributes? oldPersonAttributes;

        async Task<string?> GenerateTrnTokenIfHaveEmailAsync(string trn)
        {
            if (string.IsNullOrEmpty(requestData.EmailAddress))
            {
                return null;
            }

            return await trnRequestService.CreateTrnTokenAsync(trn, requestData.EmailAddress);
        }

        string jobId;
        if (CreatingNewRecord)
        {
            var newContactId = Guid.NewGuid();
            requestData.ResolvedPersonId = newContactId;

            var trn = await trnGenerator.GenerateTrnAsync();
            var trnToken = await GenerateTrnTokenIfHaveEmailAsync(trn);
            requestData.TrnToken = trnToken;

            jobId = await backgroundJobScheduler.EnqueueAsync<TrnRequestService>(
                trnRequestService => trnRequestService.CreateContactFromTrnRequestAsync(requestData, newContactId, trn));
            selectedPersonAttributes = null;
            oldPersonAttributes = null;
        }
        else
        {
            Debug.Assert(state.PersonId is not null);
            var existingContactId = state.PersonId!.Value;
            requestData.ResolvedPersonId = existingContactId;

            Debug.Assert(Trn is not null);
            requestData.TrnToken = await GenerateTrnTokenIfHaveEmailAsync(Trn!);

            selectedPersonAttributes = await GetPersonAttributesAsync(existingContactId);
            var attributesToUpdate = GetAttributesToUpdate();

            jobId = await backgroundJobScheduler.EnqueueAsync<TrnRequestService>(
                trnRequestService => trnRequestService.UpdateContactFromTrnRequestAsync(
                    requestData,
                    attributesToUpdate));

            oldPersonAttributes = new EventModels.TrnRequestPersonAttributes()
            {
                FirstName = selectedPersonAttributes.FirstName,
                MiddleName = selectedPersonAttributes.MiddleName,
                LastName = selectedPersonAttributes.LastName,
                DateOfBirth = selectedPersonAttributes.DateOfBirth,
                EmailAddress = selectedPersonAttributes.EmailAddress,
                NationalInsuranceNumber = selectedPersonAttributes.NationalInsuranceNumber
            };
        }

        Debug.Assert(requestData.ResolvedPersonId is not null);

        var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;
        supportTask.UpdateData<ApiTrnRequestData>(data => data with
        {
            ResolvedAttributes = resolvedPersonAttributes,
            SelectedPersonAttributes = selectedPersonAttributes
        });

        var changes = ApiTrnRequestSupportTaskUpdatedEventChanges.Status |
            (state.FirstNameSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName : 0) |
            (state.MiddleNameSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName : 0) |
            (state.LastNameSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName : 0) |
            (state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth : 0) |
            (state.EmailAddressSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress : 0) |
            (state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber : 0);

        var @event = new ApiTrnRequestSupportTaskUpdatedEvent()
        {
            PersonId = requestData.ResolvedPersonId!.Value,
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
            Changes = changes,
            PersonAttributes = new EventModels.TrnRequestPersonAttributes()
            {
                FirstName = resolvedPersonAttributes.FirstName,
                MiddleName = resolvedPersonAttributes.MiddleName,
                LastName = resolvedPersonAttributes.LastName,
                DateOfBirth = resolvedPersonAttributes.DateOfBirth,
                EmailAddress = resolvedPersonAttributes.EmailAddress,
                NationalInsuranceNumber = resolvedPersonAttributes.NationalInsuranceNumber
            },
            OldPersonAttributes = oldPersonAttributes,
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };
        await DbContext.AddEventAndBroadcastAsync(@event);

        await DbContext.SaveChangesAsync();

        // This is a little ugly but pushing this into a partial and executing it here is tricky
        var flashMessageHtml =
            $@"
            <a href=""{linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value)}"" class=""govuk-link"">View record</a>
            ";

        TempData.SetFlashSuccess(
            $"Records merged successfully for {FirstName} {MiddleName} {LastName}",
            messageHtml: flashMessageHtml);

        return Redirect(linkGenerator.ApiTrnRequests(waitForJobId: jobId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.ApiTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
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
            Debug.Assert(state.PersonId is not null);

            if (Request.Method == HttpMethod.Get.Method)
            {
                await this.TrySyncPersonAsync(personId);
            }

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
                    p.Trn
                })
                .SingleAsync();

            CreatingNewRecord = false;
            FirstName = state.FirstNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.FirstName : requestData.FirstName;
            MiddleName = state.MiddleNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.MiddleName : requestData.MiddleName;
            LastName = state.LastNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.LastName : requestData.LastName;
            DateOfBirth = state.DateOfBirthSource == PersonAttributeSource.ExistingRecord ? selectedPerson.DateOfBirth : requestData.DateOfBirth;
            EmailAddress = state.EmailAddressSource == PersonAttributeSource.ExistingRecord ? selectedPerson.EmailAddress : requestData.EmailAddress;
            NationalInsuranceNumber = state.NationalInsuranceNumberSource == PersonAttributeSource.ExistingRecord ? (string?)selectedPerson.NationalInsuranceNumber : requestData.NationalInsuranceNumber;
            Trn = selectedPerson.Trn;
        }

        Comments = state.Comments;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
