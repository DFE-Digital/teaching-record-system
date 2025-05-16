using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IClock clock,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator)
{
    private Person? _person;

    public string? Name { get; set; }
    public string? PreviousName { get; set; }
    public string? Trn { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Sex { get; set; }
    public string? MobileNumber { get; set; }
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? UploadedEvidenceFileUrl { get; set; }

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
        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess(messageText: "Personal details have been updated successfully.");

        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance.State.JourneyStartPage));
            return;
        }

        _person = await DbContext.Persons.SingleOrDefaultAsync(u => u.PersonId == PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }

        Name = StringHelper.BuildFullName(
            JourneyInstance.State.FirstName,
            JourneyInstance.State.MiddleName,
            JourneyInstance.State.LastName);
        PreviousName = StringHelper.BuildFullName(
            _person.FirstName,
            _person.MiddleName,
            _person.LastName);
        DateOfBirth = JourneyInstance.State.DateOfBirth;
        ChangeReason = JourneyInstance.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}
