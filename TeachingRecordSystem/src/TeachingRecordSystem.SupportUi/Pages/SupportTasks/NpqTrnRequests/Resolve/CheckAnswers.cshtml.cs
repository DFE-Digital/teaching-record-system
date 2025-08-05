using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        EventModels.PersonAttributes? oldPersonAttributes;

        if (CreatingNewRecord)
        {
            var trn = await trnGenerator.GenerateTrnAsync();

            var (person, _) = Person.Create(
                trn,
                requestData.FirstName ?? string.Empty,
                requestData.MiddleName ?? string.Empty,
                requestData.LastName ?? string.Empty,
                requestData.DateOfBirth,
                requestData.EmailAddress is not null ? Core.EmailAddress.Parse(requestData.EmailAddress) : null,
                requestData.NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(requestData.NationalInsuranceNumber) : null,
                requestData.Gender,
                clock.UtcNow);

            DbContext.Add(person);

            requestData.SetResolvedPerson(person.PersonId);
            selectedPersonAttributes = null;
            oldPersonAttributes = null;

            var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

            supportTask.Status = SupportTaskStatus.Closed;
            supportTask.UpdatedOn = clock.UtcNow;
            supportTask.UpdateData<NpqTrnRequestData>(data => data with
            {
                ResolvedAttributes = resolvedPersonAttributes,
                SelectedPersonAttributes = selectedPersonAttributes
            });

            var @event = new NpqTrnRequestSupportTaskUpdatedEvent()
            {
                PersonId = requestData.ResolvedPersonId!.Value,
                RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
                Changes = NpqTrnRequestSupportTaskUpdatedEventChanges.Status,
                PersonAttributes = EventModels.PersonAttributes.FromModel(person),
                OldPersonAttributes = oldPersonAttributes,
                SupportTask = EventModels.SupportTask.FromModel(supportTask),
                OldSupportTask = oldSupportTaskEventModel,
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

            oldPersonAttributes = new EventModels.PersonAttributes()
            {
                FirstName = selectedPersonAttributes.FirstName,
                MiddleName = selectedPersonAttributes.MiddleName,
                LastName = selectedPersonAttributes.LastName,
                DateOfBirth = selectedPersonAttributes.DateOfBirth,
                EmailAddress = selectedPersonAttributes.EmailAddress,
                NationalInsuranceNumber = selectedPersonAttributes.NationalInsuranceNumber,
                Gender = selectedPersonAttributes.Gender
            };

            // update the person
            var person = await DbContext.Persons.SingleAsync(p => p.PersonId == requestData.ResolvedPersonId);

            person.UpdateDetails(
                firstName: selectedPersonAttributes.FirstName,
                middleName: selectedPersonAttributes.MiddleName,
                lastName: selectedPersonAttributes.LastName,
                dateOfBirth: DateOfBirth,
                emailAddress: EmailAddress is not null ? Core.EmailAddress.Parse(EmailAddress) : null,
                nationalInsuranceNumber: NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(NationalInsuranceNumber) : null,
                Gender,
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
                (state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? NpqTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber : 0) |
                (state.GenderSource is PersonAttributeSource.TrnRequest ? NpqTrnRequestSupportTaskUpdatedEventChanges.PersonGender : 0);

            var @event = new NpqTrnRequestSupportTaskUpdatedEvent()
            {
                PersonId = requestData.ResolvedPersonId!.Value,
                SupportTask = EventModels.SupportTask.FromModel(supportTask),
                OldSupportTask = oldSupportTaskEventModel,
                RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
                Changes = changes,
                PersonAttributes = new EventModels.PersonAttributes()
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
                Comments = Comments,
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId()
            };
            await DbContext.AddEventAndBroadcastAsync(@event);
        }

        await DbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"{SourceApplicationUserName} request completed",
            buildMessageHtml: b =>
            {
                var link = new TagBuilder("a");
                link.AddCssClass("govuk-link");
                link.MergeAttribute("href", linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value));
                link.InnerHtml.Append($"Record {(CreatingNewRecord ? "created" : "updated")} for {FirstName} {MiddleName} {LastName}");
                b.AppendHtml(link);
            });

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.NpqTrnRequestMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
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
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
