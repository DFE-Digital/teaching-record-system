namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveInitialTeacherTrainingsByContactIdQuery(Guid ContactId) :
    ICrmQuery<dfeta_initialteachertraining[]>;
