using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
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

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public Gender? Gender { get; set; }

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

        //var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        NpqTrnRequestDataPersonAttributes? selectedPersonAttributes;
        //EventModels.TrnRequestPersonAttributes? oldPersonAttributes;

        // CMl TODO - check don't need this
        //async Task<string?> GenerateTrnTokenIfHaveEmailAsync(string trn)
        //{
        //    if (string.IsNullOrEmpty(requestData.EmailAddress))
        //    {
        //        return null;
        //    }

        //    return await trnRequestService.CreateTrnTokenAsync(trn, requestData.EmailAddress);
        //}

        // CML TODO - Creating is the ticket after this one
        //string jobId;
        if (CreatingNewRecord)
        {
            var newContactId = Guid.NewGuid();
            requestData.ResolvedPersonId = newContactId;

            var trn = await trnGenerator.GenerateTrnAsync();
            //var trnToken = await GenerateTrnTokenIfHaveEmailAsync(trn);
            //requestData.TrnToken = trnToken;

            //jobId = await backgroundJobScheduler.EnqueueAsync<TrnRequestService>(
            //    trnRequestService => trnRequestService.CreateContactFromTrnRequestAsync(requestData, newContactId, trn));
            selectedPersonAttributes = null;
            //oldPersonAttributes = null;
        }
        else
        {
            //Debug.Assert(state.PersonId is not null);
            var existingContactId = state.PersonId!.Value;
            requestData.ResolvedPersonId = existingContactId;

            //Debug.Assert(Trn is not null);
            //requestData.TrnToken = await GenerateTrnTokenIfHaveEmailAsync(Trn!);

            selectedPersonAttributes = await GetPersonAttributesAsync(existingContactId);
            var attributesToUpdate = GetAttributesToUpdate();

            //oldPersonAttributes = new EventModels.TrnRequestPersonAttributes()
            //{
            //    FirstName = selectedPersonAttributes.FirstName,
            //    MiddleName = selectedPersonAttributes.MiddleName,
            //    LastName = selectedPersonAttributes.LastName,
            //    DateOfBirth = selectedPersonAttributes.DateOfBirth,
            //    EmailAddress = selectedPersonAttributes.EmailAddress,
            //    NationalInsuranceNumber = selectedPersonAttributes.NationalInsuranceNumber
            //};
        }

        Debug.Assert(requestData.ResolvedPersonId is not null);

        var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;
        supportTask.UpdateData<NpqTrnRequestData>(data => data with
        {
            ResolvedAttributes = resolvedPersonAttributes,
            SelectedPersonAttributes = selectedPersonAttributes
        });

        // CML TODO updating the person here for now
        var person = await DbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == requestData.ResolvedPersonId);
        if (person == null)
        {
            throw new ArgumentException("Person not found.");
        }

        // create new PersonUpdatedFromTrnRequestEvent - add all the metadata - 
        person!.UpdateDetailsFromTrnRequest(
            dateOfBirth: DateOfBirth,
            emailAddress: EmailAddress is not null ? Core.EmailAddress.Parse(EmailAddress) : null,
            nationalInsuranceNumber: NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(NationalInsuranceNumber) : null,
            detailsChangeReasonDetail: Comments,
            detailsChangeEvidenceFile: null, // requestData.UploadedEvidence, // TODO the uploaded file from the Support task
            SourceApplicationUserId!,
            clock.UtcNow,
            requestData,
            out var updateEvent);
        await DbContext.SaveChangesAsync();
        if (updateEvent is not null)
        {
            await DbContext.AddEventAndBroadcastAsync(updateEvent);
            await DbContext.SaveChangesAsync();
        }

        // This is a little ugly but pushing this into a partial and executing it here is tricky
        var flashMessageHtml =
            $@"
            <a href=""{linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value)}"" class=""govuk-link"">View record</a>
            ";

        TempData.SetFlashSuccess(
            $"Records merged successfully for {FirstName} {MiddleName} {LastName}",
            messageHtml: flashMessageHtml);

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

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
