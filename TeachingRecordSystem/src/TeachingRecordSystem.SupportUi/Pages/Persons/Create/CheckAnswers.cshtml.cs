using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Create;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.CreatePerson), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    IFileService fileService,
    ITrnGenerator trnGenerator)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EmailAddress? EmailAddress { get; set; }
    public NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    public CreateReasonOption? CreateReason { get; set; }
    public string? ReasonDetail { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? EvidenceFileUrl { get; set; }

    public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public string? ChangePersonalDetailsLink =>
        GetPageLink(CreateJourneyPage.PersonalDetails, true);

    public string? CreateReasonLink =>
        GetPageLink(CreateJourneyPage.CreateReason, true);

    public string BackLink => GetPageLink(CreateJourneyPage.CreateReason);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        var trn = await trnGenerator.GenerateTrnAsync();

        var (person, personAttributes) = Person.Create(
            trn,
            FirstName ?? string.Empty,
            MiddleName ?? string.Empty,
            LastName ?? string.Empty,
            DateOfBirth,
            EmailAddress,
            NationalInsuranceNumber,
            Gender,
            now);

        var createdEvent = new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId(),
            PersonId = person.PersonId,
            PersonAttributes = personAttributes,
            CreateReason = CreateReason?.GetDisplayName(),
            CreateReasonDetail = ReasonDetail,
            EvidenceFile = EvidenceFileId is Guid detailsFileId
                ? new EventModels.File()
                {
                    FileId = detailsFileId,
                    Name = EvidenceFileName!
                }
                : null
        };

        DbContext.Add(person);
        await DbContext.AddEventAndBroadcastAsync(createdEvent);
        await DbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess(messageText: $"Record created successfully for {Name}.");

        return Redirect(LinkGenerator.PersonDetail(person.PersonId));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < CreateJourneyPage.CheckAnswers)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }

        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance.State.MiddleName;
        LastName = JourneyInstance.State.LastName;
        DateOfBirth = JourneyInstance.State.DateOfBirth;
        EmailAddress = JourneyInstance.State.EmailAddress.Parsed;
        NationalInsuranceNumber = JourneyInstance.State.NationalInsuranceNumber.Parsed;
        Gender = JourneyInstance.State.Gender;
        CreateReason = JourneyInstance.State.CreateReason;
        ReasonDetail = JourneyInstance.State.CreateReasonDetail;
        EvidenceFileId = JourneyInstance.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        EvidenceFileUrl = JourneyInstance.State.EvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}
