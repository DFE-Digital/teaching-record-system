using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    ITrnGenerator trnGenerator,
    TrsLinkGenerator linkGenerator,
    IClock clock) : ResolveNpqTrnRequestPageModel(dbContext)
{
    public string? SourceApplicationUserName { get; set; }
    public Guid? SourceApplicationUserId { get; set; }

    public bool CreatingNewRecord { get; set; }

    public bool? PotentialDuplicate { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public string? Trn { get; set; }

    public string? Comments { get; set; }

    public bool PersonNameChange { get; set; }
    public bool PersonDetailsChange { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        NpqTrnRequestDataPersonAttributes? selectedPersonAttributes;
        EventModels.TrnRequestPersonAttributes? oldPersonAttributes;

        if (CreatingNewRecord)
        {
            var trn = await trnGenerator.GenerateTrnAsync();
            var person = Person.Create(
                trn,
                requestData.FirstName ?? string.Empty,
                requestData.MiddleName ?? string.Empty,
                requestData.LastName ?? string.Empty,
                requestData.DateOfBirth,
                requestData.EmailAddress is not null ? Core.EmailAddress.Parse(requestData.EmailAddress) : null,
                requestData.NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(requestData.NationalInsuranceNumber) : null,
                clock.UtcNow);
            requestData.SetResolvedPerson(person.PersonId);
            selectedPersonAttributes = null;
            oldPersonAttributes = null;
            DbContext.Add(person);

            var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

            supportTask.Status = SupportTaskStatus.Closed;
            supportTask.UpdatedOn = clock.UtcNow;
            supportTask.UpdateData<NpqTrnRequestData>(data => data with
            {
                ResolvedAttributes = resolvedPersonAttributes,
                SelectedPersonAttributes = selectedPersonAttributes
            });

            var @event = new NpqTrnRequestSupportTaskCreatedPersonEvent()
            {
                PersonId = requestData.ResolvedPersonId!.Value,
                PersonDetails = EventModels.PersonDetails.FromModel(person),
                SupportTask = EventModels.SupportTask.FromModel(supportTask),
                OldSupportTask = oldSupportTaskEventModel,
                RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
                Comments = Comments,
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId()
            };

            await DbContext.AddEventAndBroadcastAsync(@event);

            await DbContext.SaveChangesAsync();
        }
        else // updating
        {
            var existingContactId = state.PersonId!.Value;
            requestData.SetResolvedPerson(existingContactId);

            selectedPersonAttributes = await GetPersonAttributesAsync(existingContactId);
            var attributesToUpdate = GetAttributesToUpdate();

            oldPersonAttributes = new EventModels.TrnRequestPersonAttributes()
            {
                FirstName = selectedPersonAttributes.FirstName,
                MiddleName = selectedPersonAttributes.MiddleName,
                LastName = selectedPersonAttributes.LastName,
                DateOfBirth = selectedPersonAttributes.DateOfBirth,
                EmailAddress = selectedPersonAttributes.EmailAddress,
                NationalInsuranceNumber = selectedPersonAttributes.NationalInsuranceNumber
            };

            // update the person
            var person = await DbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == requestData.ResolvedPersonId)
                ?? throw new ArgumentException("Person not found.");

            person!.UpdateDetailsFromTrnRequest(
                dateOfBirth: DateOfBirth,
                emailAddress: EmailAddress is not null ? Core.EmailAddress.Parse(EmailAddress) : null,
                nationalInsuranceNumber: NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(NationalInsuranceNumber) : null,
                clock.UtcNow);

            Debug.Assert(requestData.ResolvedPersonId is not null);

            var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

            supportTask.Status = SupportTaskStatus.Closed;
            supportTask.UpdatedOn = clock.UtcNow;
            supportTask.UpdateData<NpqTrnRequestData>(data => data with
            {
                ResolvedAttributes = resolvedPersonAttributes,
                SelectedPersonAttributes = selectedPersonAttributes
            });

            var changes = NpqTrnRequestSupportTaskUpdatedEventChanges.Status |
                    (state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? NpqTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth : 0) |
                    (state.EmailAddressSource is PersonAttributeSource.TrnRequest ? NpqTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress : 0) |
                    (state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? NpqTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber : 0);

            var @event = new NpqTrnRequestSupportTaskUpdatedEvent()
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
                Comments = Comments,
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId()
            };
            await DbContext.AddEventAndBroadcastAsync(@event);
        }

        await DbContext.SaveChangesAsync();
        // This is a little ugly but pushing this into a partial and executing it here is tricky
        var flashMessageHtml =
            $@"
            <a href=""{linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value)}"" class=""govuk-link"">View record</a>
            ";

        var message = CreatingNewRecord ? "Record created for" : "Records merged successfully for";
        TempData.SetFlashSuccess(
            $"{message} {FirstName} {MiddleName} {LastName}",
            messageHtml: flashMessageHtml);

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.SupportTasks());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid PersonId)
        {
            context.Result = Redirect(linkGenerator.NpqTrnRequestMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (PersonId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.NpqTrnRequestMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
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
            Trn = selectedPerson.Trn;
        }

        Comments = state.Comments;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;
        SourceApplicationUserId = requestData.ApplicationUser!.UserId;
        PotentialDuplicate = requestData.PotentialDuplicate;
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
