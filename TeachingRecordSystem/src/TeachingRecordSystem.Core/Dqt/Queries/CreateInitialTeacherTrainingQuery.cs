namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateInitialTeacherTrainingQuery : ICrmQuery<Guid>
{
    public required Guid? PersonId { get; init; }
    public required Guid? CountryId { get; init; }
    public required Guid? ITTQualificationId { get; init; }
    public required dfeta_ITTResult? Result { get; init; }
}



    
