namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class ConnectOneLoginState
{
    public string? Subject { get; set; }
    public string? OneLoginEmailAddress { get; set; }
    public IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>? MatchedAttributes { get; set; }
    public ConnectOneLoginReason? ConnectReason { get; set; }
    public string? ReasonDetail { get; set; }
}
