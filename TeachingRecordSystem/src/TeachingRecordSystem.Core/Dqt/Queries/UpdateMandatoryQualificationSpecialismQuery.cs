namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateMandatoryQualificationSpecialismQuery(Guid QualificationId, Guid SpecialismId) : ICrmQuery<bool>;
