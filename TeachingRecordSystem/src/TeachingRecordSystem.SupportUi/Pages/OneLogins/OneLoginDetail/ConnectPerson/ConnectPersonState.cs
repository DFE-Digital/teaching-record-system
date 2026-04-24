namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

public class ConnectPersonState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.ConnectPerson,
        typeof(ConnectPersonState),
        requestDataKeys: ["oneLoginUserSubject"],
        appendUniqueKey: true);

    public Guid? PersonId { get; set; }
    public string? PersonTrn { get; set; }

    public bool IsComplete => PersonId.HasValue;
}
