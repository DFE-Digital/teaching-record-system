﻿using System;
using System.IO;

namespace QualifiedTeachersApi.DataStore.Crm;

public record CreateDateOfBirthChangeIncidentCommand
{
    public required Guid ContactId { get; init; }
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required Stream EvidenceFileContent { get; init; }
    public required string EvidenceFileMimeType { get; init; }
    public required bool FromIdentity { get; init; }
}
