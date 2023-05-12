using System.Collections.Generic;

namespace QualifiedTeachersApi.Infrastructure.Security;

public class ApiClient
{
    public required string ClientId { get; set; }
    public required List<string> ApiKey { get; set; }
}
