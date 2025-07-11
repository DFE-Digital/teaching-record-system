using System.Collections.ObjectModel;

namespace TeachingRecordSystem.Core.Events.Models;

public record Email
{
    public required Guid EmailId { get; init; }
    public required string TemplateId { get; init; }
    public required string EmailAddress { get; init; }
    public required IReadOnlyDictionary<string, string> Personalization { get; init; }
    public required IReadOnlyDictionary<string, object> Metadata { get; init; }
    public required DateTime? SentOn { get; init; }

    public static Email FromModel(TeachingRecordSystem.Core.DataStore.Postgres.Models.Email email) => new()
    {
        EmailId = email.EmailId,
        TemplateId = email.TemplateId,
        EmailAddress = email.EmailAddress,
        Personalization = new ReadOnlyDictionary<string, string>(email.Personalization),
        Metadata = new ReadOnlyDictionary<string, object>(email.Metadata),
        SentOn = email.SentOn
    };
}
