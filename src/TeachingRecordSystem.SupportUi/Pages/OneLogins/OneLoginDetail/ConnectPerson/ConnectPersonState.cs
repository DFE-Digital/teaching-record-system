namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

public class ConnectPersonState
{
    public Guid? PersonId { get; set; }
    public string? PersonTrn { get; set; }
    public ConnectPersonReason? ConnectReason { get; set; }
    public string? ReasonDetail { get; set; }
}
