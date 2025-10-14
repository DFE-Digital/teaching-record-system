using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    TrnRequestService trnRequestService,
    EvidenceUploadManager evidenceController,
    IClock clock) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    public string? SourceApplicationUserName { get; set; }

    public bool MergingRecord { get; set; }

    public bool? PotentialDuplicate { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public Gender? Gender { get; set; }

    public string? Trn { get; set; }

    public string? MergeComments { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = GetSupportTask();
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.TeacherPensionsMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.TeacherPensionsMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }
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

        FirstName = state.FirstNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.FirstName : requestData.FirstName;
        MiddleName = state.MiddleNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.MiddleName : requestData.MiddleName;
        LastName = state.LastNameSource == PersonAttributeSource.ExistingRecord ? selectedPerson.LastName : requestData.LastName;
        DateOfBirth = state.DateOfBirthSource == PersonAttributeSource.ExistingRecord ? selectedPerson.DateOfBirth : requestData.DateOfBirth;
        NationalInsuranceNumber = state.NationalInsuranceNumberSource == PersonAttributeSource.ExistingRecord ? selectedPerson.NationalInsuranceNumber : requestData.NationalInsuranceNumber;
        Gender = state.GenderSource == PersonAttributeSource.ExistingRecord ? selectedPerson.Gender : requestData.Gender;
        Trn = selectedPerson.Trn; //trn cannot be changed, it'll always be the existing trn that is kept

        MergeComments = state.MergeComments;
        PotentialDuplicate = requestData.PotentialDuplicate;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        TeacherPensionsPotentialDuplicateAttributes? selectedPersonAttributes;
        EventModels.PersonAttributes? oldPersonAttributes;
        var now = clock.UtcNow;

        var existingContactId = state.PersonId!.Value;
        requestData.SetResolvedPerson(existingContactId);

        selectedPersonAttributes = await GetPersonAttributesAsync(existingContactId);
        var attributesToUpdate = GetAttributesToUpdate();
        oldPersonAttributes = new EventModels.PersonAttributes()
        {
            FirstName = selectedPersonAttributes.FirstName,
            MiddleName = selectedPersonAttributes.MiddleName,
            LastName = selectedPersonAttributes.LastName,
            DateOfBirth = selectedPersonAttributes.DateOfBirth,
            EmailAddress = null,
            NationalInsuranceNumber = selectedPersonAttributes.NationalInsuranceNumber,
            Gender = selectedPersonAttributes.Gender
        };

        var teacherPensionPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == supportTask.PersonId);
        var existingPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == requestData.ResolvedPersonId);
        var result = trnRequestService.UpdatePersonFromTrnRequest(existingPerson, requestData, attributesToUpdate, now);
        var resolvedPersonAttributes = GetResolvedPersonAttributes(selectedPersonAttributes);
        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = now;
        DbContext.Attach(supportTask);

        supportTask.UpdateData<TeacherPensionsPotentialDuplicateData>(data => data with
        {
            ResolvedAttributes = resolvedPersonAttributes,
            SelectedPersonAttributes = selectedPersonAttributes
        });

        var changes = TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.Status |
            (state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonDateOfBirth : 0) |
            (state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber : 0) |
            (state.GenderSource is PersonAttributeSource.TrnRequest ? TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonGender : 0) |
            (state.FirstNameSource is PersonAttributeSource.TrnRequest ? TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonFirstName : 0) |
            (state.MiddleNameSource is PersonAttributeSource.TrnRequest ? TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonMiddleName : 0) |
            (state.LastNameSource is PersonAttributeSource.TrnRequest ? TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonLastName : 0);

        var @event = new TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent()
        {
            PersonId = existingPerson.PersonId!,
            RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
            ChangeReason = TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordMerged,
            Changes = changes,
            PersonAttributes = EventModels.PersonAttributes.FromModel(existingPerson!),
            OldPersonAttributes = oldPersonAttributes,
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            Comments = MergeComments,
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId(),
            EvidenceFileId = state.Evidence.UploadedEvidenceFile?.FileId,
            EvidenceFileName = state.Evidence.UploadedEvidenceFile?.FileName,
            EvidenceFileSizeDescription = state.Evidence.UploadedEvidenceFile?.FileSizeDescription,
            SecondaryPersonTrn = teacherPensionPerson.Trn
        };

        teacherPensionPerson.Status = PersonStatus.Deactivated;
        await DbContext.AddEventAndBroadcastAsync(@event);
        await DbContext.SaveChangesAsync();
        TempData.SetFlashSuccess(
            $"Teachers’ Pensions duplicate task completed",
            buildMessageHtml: b =>
            {
                var link = new TagBuilder("a");
                link.AddCssClass("govuk-link");
                link.MergeAttribute("href", linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value));
                link.InnerHtml.Append($"View record.");
                var span = new TagBuilder("span");
                span.InnerHtml.Append($"Record updated for {FirstName} {LastName}. ");
                span.InnerHtml.AppendHtml(link);
                b.AppendHtml(span);
            });

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.TeacherPensions());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.TeacherPensions());
    }
}
