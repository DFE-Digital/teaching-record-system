using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

public class DisconnectOneLoginState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DisconnectOneLogin,
        typeof(DisconnectOneLoginState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public DisconnectOneLoginReason? DisconnectReason { get; set; }

    public DisconnectOneLoginStayVerified? StayVerified { get; set; }

    public string? Detail { get; set; }

}
