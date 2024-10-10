namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetInitialTeacherTrainingsByContactIdQuery(Guid ContactId) :
    ICrmQuery<dfeta_initialteachertraining[]>;
