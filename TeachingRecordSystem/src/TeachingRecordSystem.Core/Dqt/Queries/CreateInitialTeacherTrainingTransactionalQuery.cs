namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateInitialTeacherTrainingTransactionalQuery : ICrmTransactionalQuery<Guid>
{
    public Guid Id { get; set; }
    public required Guid ContactId { get; init; }
    public required Guid? CountryId { get; init; }
    public required Guid? ITTQualificationId { get; init; }
    public required dfeta_ITTResult Result { get; init; }
}
