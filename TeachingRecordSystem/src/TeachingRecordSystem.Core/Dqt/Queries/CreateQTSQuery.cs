namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateQTSQuery : ICrmQuery<Guid>
{
    public required Guid? PersonId { get; init; }
    public required Guid? TeacherStatusId { get; init; }
    public required DateTime? QTSDate { get; init; }
    //public required Guid? InductionId { get; init; }
}
