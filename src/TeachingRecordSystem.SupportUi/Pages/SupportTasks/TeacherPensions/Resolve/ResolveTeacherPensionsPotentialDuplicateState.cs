using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public record ResolveTeacherPensionsPotentialDuplicateState : IJourneyWithSavedState
{
    /// <summary>
    /// The value the Matches page submits for "keep the records separate" rather than picking a record.
    /// </summary>
    public static Guid KeepRecordSeparatePersonIdSentinel => Guid.Empty;

    public required IReadOnlyCollection<MatchPersonsResultPerson> MatchedPersons { get; init; }

    /// <summary>
    /// Where the user is sent when the journey finishes or is cancelled; the page the journey was
    /// started from, or the Teachers' Pensions list when it was started without one.
    /// </summary>
    public required string CompletionUrl { get; init; }

    public SavedJourneyState? SavedJourneyState { get; set; }
    public Guid? PersonId { get; set; }
    public bool PersonAttributeSourcesSet { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public PersonAttributeSource? GenderSource { get; set; }
    public string? Reason { get; set; }
    public string? MergeComments { get; set; }
    public PersonAttributeSource? TRNSource { get; set; }
    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }
    public Guid? TeachersPensionPersonId { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();
}
