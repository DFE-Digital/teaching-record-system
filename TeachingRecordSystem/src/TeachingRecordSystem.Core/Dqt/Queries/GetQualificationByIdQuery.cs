namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetQualificationByIdQuery(Guid QualificationId) : ICrmQuery<dfeta_qualification?>;
