using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

public class RequestTrnJourneyState()
{
    public const string JourneyName = "RequestTrnJourney";

    public static JourneyDescriptor JourneyDescriptor { get; } =
        new JourneyDescriptor(JourneyName, typeof(RequestTrnJourneyState), requestDataKeys: [], appendUniqueKey: true);

    public string? WorkEmail { get; set; }
    public string? Name { get; set; }
    public bool? HasPreviousName { get; set; }
    public string? PreviousName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public bool? HasNationalInsuranceNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? TownOrCity { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool HasPendingTrnRequest { get; set; }
    public bool? IsTakingNpq { get; set; }
    public string? NpqApplicationId { get; set; }
    public string? NpqTrainingProvider { get; set; }
    public string? PersonalEmail { get; set; }
    public bool? WorkingInSchoolOrEducationalSetting { get; set; }
    public bool? HaveRegisteredForAnNpq { get; set; }
    public string? NpqName { get; set; }
}
