#nullable disable
using System.Collections.Generic;

namespace QualifiedTeachersApi.Security;

public class ApiClient
{
    public string ClientId { get; set; }
    public List<string> ApiKey { get; set; }
}
