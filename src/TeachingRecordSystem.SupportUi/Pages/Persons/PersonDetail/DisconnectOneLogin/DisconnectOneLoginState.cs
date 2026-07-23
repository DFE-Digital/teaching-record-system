using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

public class DisconnectOneLoginState
{
    public DisconnectOneLoginReason? DisconnectReason { get; set; }

    public DisconnectOneLoginStayVerified? StayVerified { get; set; }

    public string? Detail { get; set; }
}
