namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateHeQualificationTransactionalQuery : ICrmTransactionalQuery<Guid>
{
    public required Guid Id { get; init; }
    public required Guid ContactId { get; init; }
    public required Guid? HECountryId { get; init; }
    public required string? HECourseLength { get; init; }
    public required Guid? HEEstablishmentId { get; init; }
    public required Guid? HEQualificationId { get; init; }
    public required dfeta_classdivision? HEClassDivision { get; init; }
    public required Guid? HESubject1id { get; init; }
    public required Guid? HESubject2id { get; init; }
    public required Guid? HESubject3id { get; init; }
    public required dfeta_qualification_dfeta_Type? Type { get; init; }
}
