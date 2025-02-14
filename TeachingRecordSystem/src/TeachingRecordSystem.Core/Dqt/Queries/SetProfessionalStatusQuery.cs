namespace TeachingRecordSystem.Core.Dqt.Queries;

public record SetProfessionalStatusQuery(
    Guid ContactId,
    string Trn,
    bool HasActiveAlert,
    dfeta_initialteachertraining Itt,
    bool IsNewItt,
    dfeta_qtsregistration QtsRegistration,
    bool IsNewQts,
    dfeta_TrsOutboxMessage? InductionOutboxMessage) : ICrmQuery<bool>;
