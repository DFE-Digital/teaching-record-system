using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[RequireFeatureEnabledFilterFactory(FeatureNames.NewPersonDetails)]
[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    private Person? _person;

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Trn { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EmailAddress? EmailAddress { get; set; }
    public MobileNumber? MobileNumber { get; set; }
    public NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? UploadedEvidenceFileUrl { get; set; }

    public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public string? ChangePersonalDetailsLink =>
        GetPageLink(EditDetailsJourneyPage.Index, true);

    public string? ChangeChangeReasonLink =>
        GetPageLink(EditDetailsJourneyPage.ChangeReason, true);

    public string BackLink => GetPageLink(EditDetailsJourneyPage.ChangeReason);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _person!.UpdateDetails(
            FirstName!,
            MiddleName,
            LastName!,
            DateOfBirth,
            (string?)EmailAddress,
            (string?)MobileNumber,
            (string?)NationalInsuranceNumber,
            ChangeReason!.GetDisplayName()!,
            ChangeReasonDetail!,
            JourneyInstance!.State.EvidenceFileId is Guid fileId
                ? new EventModels.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.EvidenceFileName!
                }
                : null,
            User.GetUserId(),
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await DbContext.AddEventAndBroadcastAsync(updatedEvent);
            await DbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess(messageText: "Personal details have been updated successfully.");

        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(GetPageLink(EditDetailsJourneyPage.Index));
            return;
        }

        _person = await DbContext.Persons.SingleOrDefaultAsync(u => u.PersonId == PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }

        Trn = _person.Trn;
        FirstName = JourneyInstance.State.FirstName;
        MiddleName = JourneyInstance.State.MiddleName;
        LastName = JourneyInstance.State.LastName;
        DateOfBirth = JourneyInstance.State.DateOfBirth;
        MobileNumber = JourneyInstance.State.MobileNumber;
        EmailAddress = JourneyInstance.State.EmailAddress;
        NationalInsuranceNumber = JourneyInstance.State.NationalInsuranceNumber;
        ChangeReason = JourneyInstance.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance.State.EvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}
