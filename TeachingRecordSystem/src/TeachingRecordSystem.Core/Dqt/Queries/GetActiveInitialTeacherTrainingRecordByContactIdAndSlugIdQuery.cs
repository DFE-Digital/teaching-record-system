namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveInitialTeacherTrainingRecordByContactIdAndSlugIdQuery(Guid ContactId, string SlugId) :
    ICrmQuery<dfeta_initialteachertraining?>;
