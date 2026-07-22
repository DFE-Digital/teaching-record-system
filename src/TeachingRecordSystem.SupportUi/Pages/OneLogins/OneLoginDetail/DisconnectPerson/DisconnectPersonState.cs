using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

public class DisconnectPersonState
{
    public DisconnectPersonReason? DisconnectReason { get; set; }

    public DisconnectPersonStayVerified? StayVerified { get; set; }

    public string? Detail { get; set; }
}
