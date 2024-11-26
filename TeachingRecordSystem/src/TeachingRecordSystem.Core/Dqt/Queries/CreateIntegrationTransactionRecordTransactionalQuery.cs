namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateIntegrationTransactionRecordTransactionalQuery : ICrmTransactionalQuery<Guid>
{
    public required Guid IntegrationTransactionId { get; init; }
    public required string? Reference { get; init; }
    public required Guid? ContactId { get; init; }
    public required Guid? InitialTeacherTrainingId { get; init; }
    public required Guid? QualificationId { get; init; }
    public required Guid? InductionId { get; init; }
    public required Guid? InductionPeriodId { get; init; }
    public required dfeta_integrationtransactionrecord_dfeta_DuplicateStatus? DuplicateStatus { get; init; }
    public required dfeta_integrationtransactionrecord_StatusCode? StatusCode { get; init; }
    public required string? FailureMessage { get; init; }
    public required string? RowData { get; init; }
    public required string? FileName { get; set; }
}
