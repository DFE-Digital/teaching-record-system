namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetQualificationsByContactIdQuery(Guid ContactId) : ICrmQuery<dfeta_qualification[]>;
