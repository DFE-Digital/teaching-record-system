using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Notify;

public class NotifyOptions
{
    [Required]
    public required string ApiKey { get; set; }

    public bool ApplyDomainFiltering { get; set; }

    public string[] DomainAllowList { get; set; } = Array.Empty<string>();

    public string? NoSendApiKey { get; set; }
}
