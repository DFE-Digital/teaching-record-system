using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateMandatoryQualificationQuery : ICrmQuery<bool>
{
    public required Guid QualificationId { get; init; }
    public required Guid ContactId { get; init; }
    public required Guid MqEstablishmentId { get; init; }
    public required Guid SpecialismId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required dfeta_qualification_dfeta_MQ_Status Status { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EventBase Event { get; init; }
}
