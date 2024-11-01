namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateQualificationQuery : ICrmQuery<Guid>
{
    public required Guid? PersonId { get; init; }

    public required Guid? HECountryId { get; init; }
    public required string? HECourseLength { get; init; }
    public required Guid? HEEstablishmentId { get; init; }
    public required string? PqClassCode { get; init; }
    public required Guid? HEQualificationId { get; init; }
    public required Guid? HEClassDivision { get; init; }
    public required Guid? HESubject1id { get; init; }
    public required Guid? HESubject2id { get; init; }
    public required Guid? HESubject3id { get; init; }
    public required int? Type { get; init; }
}

