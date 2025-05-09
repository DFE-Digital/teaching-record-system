using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public class EditDetailsState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditDetails,
        typeof(EditDetailsState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public EditDetailsJourneyPage? JourneyStartPage { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool IsComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue &&
        ChangeReason.HasValue &&
        (ChangeReason.Value != EditDetailsChangeReasonOption.AnotherReason || !string.IsNullOrEmpty(ChangeReasonDetail)) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public async Task EnsureInitializedAsync(TrsDbContext dbContext, Guid personId, EditDetailsJourneyPage startPage)
    {
        if (Initialized)
        {
            return;
        }

        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == personId);

        JourneyStartPage = startPage;
        FirstName = person.FirstName;
        MiddleName = person.MiddleName;
        LastName = person.LastName;
        DateOfBirth = person.DateOfBirth;

        Initialized = true;
    }
}
