using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.ManualMergePerson), RequireJourneyInstance]
public class MergeModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    public string BackLink => GetPageLink(ManualMergeJourneyPage.Matches);

    public PersonAttribute<string>? Trn { get; set; }
    public PersonAttribute<string>? FirstName { get; set; }
    public PersonAttribute<string>? MiddleName { get; set; }
    public PersonAttribute<string>? LastName { get; set; }
    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }
    public PersonAttribute<string?>? EmailAddress { get; set; }
    public PersonAttribute<string?>? NationalInsuranceNumber { get; set; }

    [BindProperty]
    [Display(Name = "First name")]
    public PersonAttributeSource? FirstNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Middle name")]
    public PersonAttributeSource? MiddleNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Last name")]
    public PersonAttributeSource? LastNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Date of birth")]
    public PersonAttributeSource? DateOfBirthSource { get; set; }

    [BindProperty]
    [Display(Name = "Email")]
    public PersonAttributeSource? EmailAddressSource { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number")]
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public Guid? EvidenceFileId { get; set; }

    [BindProperty]
    public string? EvidenceFileName { get; set; }

    [BindProperty]
    public string? EvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? EvidenceFileUrl { get; set; }

    [BindProperty]
    [Display(Name = "Add comments (optional)")]
    public string? Comments { get; set; }

    private IReadOnlyList<PotentialDuplicate>? _potentialDuplicates;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.PersonAId is not Guid personAId || JourneyInstance!.State.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(ManualMergeJourneyPage.EnterTrn));
            return;
        }

        if (JourneyInstance!.State.PrimaryRecordId is not Guid primaryRecordId)
        {
            context.Result = Redirect(GetPageLink(ManualMergeJourneyPage.Matches));
            return;
        }

        _potentialDuplicates = await GetPotentialDuplicatesAsync(personAId, personBId);

        var secondaryRecordId = primaryRecordId == personAId ? personBId : personAId;

        var primaryRecordAttributes = _potentialDuplicates.Single(p => p.PersonId == primaryRecordId).Attributes;
        var secondaryRecordAttributes = _potentialDuplicates.Single(p => p.PersonId == secondaryRecordId).Attributes;

        var attributeMatches = GetPersonAttributeMatches(
            primaryRecordAttributes,
            secondaryRecordAttributes.FirstName,
            secondaryRecordAttributes.MiddleName,
            secondaryRecordAttributes.LastName,
            secondaryRecordAttributes.DateOfBirth,
            secondaryRecordAttributes.EmailAddress,
            secondaryRecordAttributes.NationalInsuranceNumber);

        Trn = new PersonAttribute<string>(
            primaryRecordAttributes.Trn,
            secondaryRecordAttributes.FirstName,
            Different: true);

        FirstName = new PersonAttribute<string>(
            primaryRecordAttributes.FirstName,
            secondaryRecordAttributes.FirstName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.FirstName));

        MiddleName = new PersonAttribute<string>(
            primaryRecordAttributes.MiddleName,
            secondaryRecordAttributes.MiddleName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.MiddleName));

        LastName = new PersonAttribute<string>(
            primaryRecordAttributes.LastName,
            secondaryRecordAttributes.LastName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.LastName));

        DateOfBirth = new PersonAttribute<DateOnly?>(
            primaryRecordAttributes.DateOfBirth,
            secondaryRecordAttributes.DateOfBirth,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.DateOfBirth));

        EmailAddress = new PersonAttribute<string?>(
            primaryRecordAttributes.EmailAddress,
            secondaryRecordAttributes.EmailAddress,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.EmailAddress));

        NationalInsuranceNumber = new PersonAttribute<string?>(
            primaryRecordAttributes.NationalInsuranceNumber,
            secondaryRecordAttributes.NationalInsuranceNumber,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
    }

    public async Task OnGetAsync()
    {
        FirstNameSource = JourneyInstance!.State.FirstNameSource;
        MiddleNameSource = JourneyInstance!.State.MiddleNameSource;
        LastNameSource = JourneyInstance!.State.LastNameSource;
        DateOfBirthSource = JourneyInstance!.State.DateOfBirthSource;
        EmailAddressSource = JourneyInstance!.State.EmailAddressSource;
        NationalInsuranceNumberSource = JourneyInstance!.State.NationalInsuranceNumberSource;
        Comments = JourneyInstance!.State.Comments;
        UploadEvidence = JourneyInstance!.State.UploadEvidence;
        EvidenceFileId = JourneyInstance.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance.State.EvidenceFileSizeDescription;
        EvidenceFileUrl = JourneyInstance.State.EvidenceFileId is null ? null :
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_potentialDuplicates!.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        if (FirstName!.Different && FirstNameSource is null)
        {
            ModelState.AddModelError(nameof(FirstNameSource), "Select a first name");
        }

        if (MiddleName!.Different && MiddleNameSource is null)
        {
            ModelState.AddModelError(nameof(MiddleNameSource), "Select a middle name");
        }

        if (LastName!.Different && LastNameSource is null)
        {
            ModelState.AddModelError(nameof(LastNameSource), "Select a last name");
        }

        if (DateOfBirth!.Different && DateOfBirthSource is null)
        {
            ModelState.AddModelError(nameof(DateOfBirthSource), "Select a date of birth");
        }

        if (EmailAddress!.Different && EmailAddressSource is null)
        {
            ModelState.AddModelError(nameof(EmailAddressSource), "Select an email");
        }

        if (NationalInsuranceNumber!.Different && NationalInsuranceNumberSource is null)
        {
            ModelState.AddModelError(nameof(NationalInsuranceNumberSource), "Select a National Insurance number");
        }

        if (UploadEvidence == true && EvidenceFileId is null && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (EvidenceFileId.HasValue && (EvidenceFile is not null || UploadEvidence != true))
        {
            await FileService.DeleteFileAsync(EvidenceFileId.Value);
            EvidenceFileName = null;
            EvidenceFileSizeDescription = null;
            EvidenceFileUrl = null;
            EvidenceFileId = null;
        }

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence == true && EvidenceFile is not null)
        {
            using var stream = EvidenceFile.OpenReadStream();
            var evidenceFileId = await FileService.UploadFileAsync(stream, EvidenceFile.ContentType);
            EvidenceFileId = evidenceFileId;
            EvidenceFileName = EvidenceFile?.FileName;
            EvidenceFileSizeDescription = EvidenceFile?.Length.Bytes().Humanize();
            EvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.FirstNameSource = FirstNameSource;
            state.MiddleNameSource = MiddleNameSource;
            state.LastNameSource = LastNameSource;
            state.DateOfBirthSource = DateOfBirthSource;
            state.EmailAddressSource = EmailAddressSource;
            state.NationalInsuranceNumberSource = NationalInsuranceNumberSource;
            state.PersonAttributeSourcesSet = true;
            state.UploadEvidence = UploadEvidence;
            state.EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            state.EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            state.EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;
            state.Comments = Comments;
        });

        return Redirect(GetPageLink(ManualMergeJourneyPage.CheckAnswers));
    }
}
