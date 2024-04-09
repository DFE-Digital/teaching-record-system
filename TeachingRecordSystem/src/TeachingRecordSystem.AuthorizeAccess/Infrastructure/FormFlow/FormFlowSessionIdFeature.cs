namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;

public class FormFlowSessionIdFeature(string sessionId)
{
    public string SessionId { get; } = sessionId;
}
