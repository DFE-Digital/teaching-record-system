namespace TeachingRecordSystem.Api.Infrastructure.Security;

public class ApiClient
{
    public required string ClientId { get; set; }
    public required List<string> ApiKey { get; set; }
    public required List<string> Roles { get; set; }
}
