#nullable disable
using System.Collections.Generic;

namespace QualifiedTeachersApi.Infrastructure.Security;

public class ApiClient
{
    public string ClientId { get; set; }
    public List<string> ApiKey { get; set; }
}
