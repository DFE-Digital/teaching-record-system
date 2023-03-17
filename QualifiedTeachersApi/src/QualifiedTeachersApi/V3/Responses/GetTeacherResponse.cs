using System;
using System.Collections.Generic;
using QualifiedTeachersApi.V3.ApiModels;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Responses;

public record GetTeacherResponse
{
    [SwaggerSchema(Nullable = false)]
    public required string Trn { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string FirstName { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string LastName { get; init; }
    public GetTeacherResponseQts Qts { get; init; }
    public GetTeacherResponseEyts Eyts { get; init; }
    public GetTeacherResponseInduction Induction { get; init; }
    public IEnumerable<GetTeacherResponseInitialTeacherTraining> InitialTeacherTraining { get; init; }
    public IEnumerable<GetTeacherResponseNpqQualificationsQualification> NpqQualifications { get; init; }
}

public record GetTeacherResponseQts
{
    public DateOnly Awarded { get; init; }
    public string CertificateUrl { get; init; }
}

public record GetTeacherResponseEyts
{
    public DateOnly Awarded { get; init; }
    public string CertificateUrl { get; init; }
}

public record GetTeacherResponseInduction
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public InductionStatus? Status { get; init; }
    public IEnumerable<GetTeacherResponseInductionPeriod> Periods { get; init; }

}

public record GetTeacherResponseInductionPeriod
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int? Terms { get; init; }
    public GetTeacherResponseInductionPeriodAppropriateBody AppropriateBody { get; init; }
}

public record GetTeacherResponseInductionPeriodAppropriateBody
{
    public string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTraining
{
    public GetTeacherResponseInitialTeacherTrainingQualification Qualification { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public IttProgrammeType? ProgrammeType { get; init; }
    public IttOutcome? Result { get; init; }
    public GetTeacherResponseInitialTeacherTrainingAgeRange AgeRange { get; init; }
    public GetTeacherResponseInitialTeacherTrainingProvider Provider { get; init; }
    public IEnumerable<GetTeacherResponseInitialTeacherTrainingSubject> Subjects { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingQualification
{
    public string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public string Description { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingProvider
{
    public string Name { get; init; }
    public string Ukprn { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingSubject
{
    public string Code { get; init; }
    public string Name { get; init; }
}

public record GetTeacherResponseNpqQualificationsQualification
{
    public DateOnly Awarded { get; init; }
    public GetTeacherResponseNpqQualificationsQualificationType Type { get; init; }
    public string CertificateUrl { get; init; }
}

public record GetTeacherResponseNpqQualificationsQualificationType
{
    public NpqQualificationType Code { get; init; }
    public string Name { get; init; }
}
