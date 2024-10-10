namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateQtsRegistrationQuery : ICrmTransactionalQuery<Guid>
{
    public required Guid Id { get; init; }
    public required Guid ContactId { get; init; }
    public required Guid TeacherStatusId { get; init; }
    public required DateTime? QtsDate { get; init; }
}
