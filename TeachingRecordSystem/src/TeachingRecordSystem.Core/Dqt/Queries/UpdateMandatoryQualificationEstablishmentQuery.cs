using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateMandatoryQualificationEstablishmentQuery(
    Guid QualificationId,
    Guid MqEstablishmentId,
    EventBase Event) : ICrmQuery<bool>;
