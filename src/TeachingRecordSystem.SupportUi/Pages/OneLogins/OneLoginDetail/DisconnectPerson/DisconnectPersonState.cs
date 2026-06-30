using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

public class DisconnectPersonState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DisconnectPerson,
        typeof(DisconnectPersonState),
        requestDataKeys: ["oneLoginUserSubject"],
        appendUniqueKey: true);

    public DisconnectPersonReason? DisconnectReason { get; set; }

    public DisconnectPersonStayVerified? StayVerified { get; set; }

    public string? Detail { get; set; }
}
