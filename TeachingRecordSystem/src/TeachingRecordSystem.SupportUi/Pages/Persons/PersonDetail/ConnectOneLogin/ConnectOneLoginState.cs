using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class ConnectOneLoginState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.ConnectOneLogin,
        typeof(ConnectOneLoginState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public string? Subject { get; set; }
    public string? OneLoginEmailAddress { get; set; }
    public MatchPersonResult? MatchedPerson { get; set; }
    public ConnectOneLoginReason? ConnectReason { get; set; }
    public string? ReasonDetail { get; set; }

    public bool IsComplete => !string.IsNullOrEmpty(Subject) && ConnectReason.HasValue;
}

