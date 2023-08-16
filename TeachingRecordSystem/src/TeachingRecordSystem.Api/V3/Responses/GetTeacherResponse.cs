using System.Text.Json.Serialization;
using Optional;
using TeachingRecordSystem.Api.V3.ApiModels;

namespace TeachingRecordSystem.Api.V3.Responses;

public record GetTeacherResponse
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Option<bool> PendingNameChange { get; init; }
    public required Option<bool> PendingDateOfBirthChange { get; init; }
    public required string? Email { get; set; }
    public required GetTeacherResponseQts? Qts { get; init; }
    public required GetTeacherResponseEyts? Eyts { get; init; }
    public required Option<GetTeacherResponseInduction?> Induction { get; init; }
    public required Option<IEnumerable<GetTeacherResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IEnumerable<GetTeacherResponseNpqQualificationsQualification>> NpqQualifications { get; init; }
    public required Option<IEnumerable<GetTeacherResponseMandatoryQualificationsQualification>> MandatoryQualifications { get; init; }
    public required Option<IEnumerable<GetTeacherResponseHigherEducationQualificationsQualification>> HigherEducationQualifications { get; init; }
    public required Option<IEnumerable<string>> Sanctions { get; init; }
}

public record GetTeacherResponseQts
{
    public required DateOnly Awarded { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
}

public record GetTeacherResponseEyts
{
    public required DateOnly Awarded { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
}

public record GetTeacherResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required InductionStatus? Status { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
    public required IEnumerable<GetTeacherResponseInductionPeriod> Periods { get; init; }
}

public record GetTeacherResponseInductionPeriod
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required int? Terms { get; init; }
    public required GetTeacherResponseInductionPeriodAppropriateBody AppropriateBody { get; init; }
}

public record GetTeacherResponseInductionPeriodAppropriateBody
{
    public required string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTraining
{
    public required GetTeacherResponseInitialTeacherTrainingQualification? Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required string? ProgrammeTypeDescription { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required IEnumerable<GetTeacherResponseInitialTeacherTrainingSubject> Subjects { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

public record GetTeacherResponseNpqQualificationsQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetTeacherResponseNpqQualificationsQualificationType Type { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
}

public record GetTeacherResponseNpqQualificationsQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

public record GetTeacherResponseMandatoryQualificationsQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

public record GetTeacherResponseHigherEducationQualificationsQualification
{
    public required string Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IEnumerable<GetTeacherResponseHigherEducationQualificationsQualificationSubject> Subjects { get; init; }
}

public record GetTeacherResponseHigherEducationQualificationsQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}
