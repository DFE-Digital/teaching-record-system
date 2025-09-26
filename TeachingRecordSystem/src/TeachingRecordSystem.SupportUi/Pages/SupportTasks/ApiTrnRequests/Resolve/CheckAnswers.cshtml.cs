using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance]
public class CheckAnswers(
    TrsDbContext dbContext,
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

    public Gender? Gender { get; set; }

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
        EventModels.PersonAttributes? oldPersonAttributes;

        var now = clock.UtcNow;

        async Task<string?> GenerateTrnTokenIfHaveEmailAsync(string trn)
        {
            if (string.IsNullOrEmpty(requestData.EmailAddress))
            {
                return null;
            }

            return await trnRequestService.CreateTrnTokenAsync(trn, requestData.EmailAddress);
        }

        if (CreatingNewRecord)
        {
            var trn = await trnGenerator.GenerateTrnAsync();
            var trnToken = await GenerateTrnTokenIfHaveEmailAsync(trn);
            requestData.TrnToken = trnToken;

            var (newPerson, _) = trnRequestService.CreatePersonFromTrnRequest(requestData, trn, now);
            DbContext.Persons.Add(newPerson);

            requestData.SetResolvedPerson(newPerson.PersonId);

            selectedPersonAttributes = null;
            oldPersonAttributes = null;
        }
        else
        {
            Debug.Assert(state.PersonId is not null);
            var existingPersonId = state.PersonId!.Value;

            var furtherChecksNeeded = await trnRequestService.RequiresFurtherChecksNeededSupportTaskAsync(
                existingPersonId,
                requestData.ApplicationUserId);

            requestData.SetResolvedPerson(existingPersonId, furtherChecksNeeded ? TrnRequestStatus.Pending : TrnRequestStatus.Completed);

            if (furtherChecksNeeded)
            {
                var furtherChecksSupportTask = SupportTask.Create(
                    SupportTaskType.TrnRequestManualChecksNeeded,
                    new TrnRequestManualChecksNeededData(),
                    existingPersonId,
                    requestData.OneLoginUserSubject,
                    requestData.ApplicationUserId,
                    requestData.RequestId,
                    User.GetUserId(),
                    now,
                    out var furtherChecksSupportTaskCreatedEvent);

                DbContext.SupportTasks.Add(furtherChecksSupportTask);
                await DbContext.AddEventAndBroadcastAsync(furtherChecksSupportTaskCreatedEvent);
            }

            Debug.Assert(Trn is not null);
            requestData.TrnToken = await GenerateTrnTokenIfHaveEmailAsync(Trn!);

            var selectedPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == existingPersonId);
            selectedPersonAttributes = GetPersonAttributes(selectedPerson);
            var attributesToUpdate = GetAttributesToUpdate();

            var updateResult = trnRequestService.UpdatePersonFromTrnRequest(selectedPerson, requestData, attributesToUpdate, now);
            oldPersonAttributes = updateResult.OldPersonAttributes;
        }

        Debug.Assert(requestData.ResolvedPersonId is not null);

        var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = now;
        supportTask.UpdateData<ApiTrnRequestData>(data => data with
        {
            ResolvedAttributes = resolvedPersonAttributes,
            SelectedPersonAttributes = selectedPersonAttributes
        });

        var changes = ApiTrnRequestSupportTaskUpdatedEventChanges.Status;

        if (!CreatingNewRecord)
        {
            changes |=
                (state.FirstNameSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName : 0) |
                (state.MiddleNameSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName : 0) |
                (state.LastNameSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName : 0) |
                (state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth : 0) |
                (state.EmailAddressSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress : 0) |
                (state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber : 0) |
                (state.GenderSource is PersonAttributeSource.TrnRequest ? ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender : 0);
        }

        var @event = new ApiTrnRequestSupportTaskUpdatedEvent()
        {
            PersonId = requestData.ResolvedPersonId!.Value,
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
            Changes = changes,
            PersonAttributes = new EventModels.PersonAttributes
            {
                FirstName = resolvedPersonAttributes.FirstName,
                MiddleName = resolvedPersonAttributes.MiddleName,
                LastName = resolvedPersonAttributes.LastName,
                DateOfBirth = resolvedPersonAttributes.DateOfBirth,
                EmailAddress = resolvedPersonAttributes.EmailAddress,
                NationalInsuranceNumber = resolvedPersonAttributes.NationalInsuranceNumber,
                Gender = resolvedPersonAttributes.Gender
            },
            OldPersonAttributes = oldPersonAttributes,
            Comments = state.Comments,
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId()
        };
        await DbContext.AddEventAndBroadcastAsync(@event);

        await DbContext.SaveChangesAsync();

        TempData.SetFlashSuccessWithLinkToRecord(
            $"{(CreatingNewRecord ? "Record created" : "Records merged")} for {StringHelper.JoinNonEmpty(' ', [FirstName, MiddleName, LastName])}",
            linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value));

        return Redirect(linkGenerator.ApiTrnRequests());
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
