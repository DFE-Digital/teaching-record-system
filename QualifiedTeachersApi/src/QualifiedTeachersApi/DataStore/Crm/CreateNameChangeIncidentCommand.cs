using System;
using System.IO;

namespace QualifiedTeachersApi.DataStore.Crm;

public record CreateNameChangeIncidentCommand
{
    public required Guid ContactId { get; init; }
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required Stream EvidenceFileContent { get; init; }
    public required string EvidenceFileMimeType { get; init; }
}
