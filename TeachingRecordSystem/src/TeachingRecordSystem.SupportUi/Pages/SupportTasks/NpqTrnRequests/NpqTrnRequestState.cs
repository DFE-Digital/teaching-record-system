namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

// CML TODO prob need most of this but I don't understand it yet - it's a copy of the API state
public class NpqTrnRequestState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.NpqTrnRequest,
        typeof(NpqTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public Guid? PersonId { get; set; }
    public bool PersonAttributeSourcesSet { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? EmailAddressSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public string? Comments { get; set; }

    public enum PersonAttributeSource
    {
        ExistingRecord = 0,
        TrnRequest = 1
    }
}
