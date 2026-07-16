using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    TeacherPensionsSupportTaskService teacherPensionsSupportTaskService,
    EvidenceUploadManager evidenceController,
    TimeProvider timeProvider,
    PersonChangeableAttributesService changedService) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
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

    public IEnumerable<ResolvedAttribute>? ResolvableAttributes { get; private set; }

    public bool IsGenderChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.Gender) == true;

    public bool IsFirstNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.FirstName) == true;

    public bool IsMiddleNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.MiddleName) == true;

    public bool IsLastNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.LastName) == true;

    public bool IsDateOfBirthChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.DateOfBirth) == true;

    public bool IsNationalInsuranceNumberChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.NationalInsuranceNumber) == true;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = GetSupportTask();
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (personId != CreateNewRecordPersonIdSentinel && !state.PersonAttributeSourcesSet)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.Merge(SupportTaskReference!, JourneyInstance!.InstanceId));
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

        // Mirrors GetResolvedPersonAttributes: only a TrnRequest source changes the record.
        FirstName = state.FirstNameSource is PersonAttributeSource.TrnRequest ? requestData.FirstName : selectedPerson.FirstName;
        MiddleName = state.MiddleNameSource is PersonAttributeSource.TrnRequest ? requestData.MiddleName : selectedPerson.MiddleName;
        LastName = state.LastNameSource is PersonAttributeSource.TrnRequest ? requestData.LastName : selectedPerson.LastName;
        DateOfBirth = state.DateOfBirthSource is PersonAttributeSource.TrnRequest ? requestData.DateOfBirth : selectedPerson.DateOfBirth;
        NationalInsuranceNumber = state.NationalInsuranceNumberSource is PersonAttributeSource.TrnRequest ? requestData.NationalInsuranceNumber : selectedPerson.NationalInsuranceNumber;
        Gender = state.GenderSource is PersonAttributeSource.TrnRequest ? requestData.Gender : selectedPerson.Gender;
        Trn = selectedPerson.Trn; //trn cannot be changed, it'll always be the existing trn that is kept

        MergeComments = state.MergeComments;
        PotentialDuplicate = requestData.PotentialDuplicate;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;

        ResolvableAttributes = changedService.GetResolvableAttributes(
        [
            new ResolvedAttribute(PersonMatchedAttribute.Gender, state.GenderSource),
            new ResolvedAttribute(PersonMatchedAttribute.FirstName, state.FirstNameSource),
            new ResolvedAttribute(PersonMatchedAttribute.MiddleName, state.MiddleNameSource),
            new ResolvedAttribute(PersonMatchedAttribute.LastName, state.LastNameSource),
            new ResolvedAttribute(PersonMatchedAttribute.DateOfBirth, state.DateOfBirthSource),
            new ResolvedAttribute(PersonMatchedAttribute.NationalInsuranceNumber, state.NationalInsuranceNumberSource)
        ]);

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var processContext = new ProcessContext(ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithMerge, timeProvider.UtcNow, User.GetUserId());

        var existingPersonId = JourneyInstance!.State.PersonId!.Value;

        await teacherPensionsSupportTaskService.ResolveWithMergeAsync(
            new()
            {
                SupportTaskReference = SupportTaskReference,
                ExistingPersonId = existingPersonId,
                AttributeSources = GetPersonAttributeSources(),
                Comments = MergeComments
            },
            processContext);

        TempData.SetFlashNotificationBanner(
            "Teachers’ Pensions duplicate task completed",
            buildMessageHtml: b =>
            {
                var link = new TagBuilder("a");
                link.AddCssClass("govuk-link");
                link.MergeAttribute("href", linkGenerator.Persons.PersonDetail.Index(existingPersonId));
                link.InnerHtml.Append($"View record.");
                var span = new TagBuilder("span");
                span.InnerHtml.Append($"Record updated for {FirstName} {LastName}. ");
                span.InnerHtml.AppendHtml(link);
                b.AppendHtml(span);
            });

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }
}
