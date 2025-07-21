namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

public class ResolveNpqTrnRequestState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveNpqTrnRequest,
        typeof(ResolveNpqTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public Guid? PersonId { get; set; }
    public bool PersonAttributeSourcesSet { get; set; }
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
