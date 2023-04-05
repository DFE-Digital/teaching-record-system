using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using QualifiedTeachersApi.V3.ApiModels;
using QualifiedTeachersApi.V3.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Responses;

public record GetTeacherResponse : IConditionallySerializedProperties
{
    [JsonIgnore]
    public GetTeacherRequestIncludes Include { get; init; }

    [SwaggerSchema(Nullable = false)]
    public required string Trn { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string FirstName { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string NationalInsuranceNumber { get; init; }
    public required bool PendingNameChange { get; init; }
    public required bool PendingDateOfBirthChange { get; init; }
    public required GetTeacherResponseQts Qts { get; init; }
    public required GetTeacherResponseEyts Eyts { get; init; }
    public required GetTeacherResponseInduction Induction { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required IEnumerable<GetTeacherResponseInitialTeacherTraining> InitialTeacherTraining { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required IEnumerable<GetTeacherResponseNpqQualificationsQualification> NpqQualifications { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required IEnumerable<GetTeacherResponseMandatoryQualificationsQualification> MandatoryQualifications { get; init; }

    public bool ShouldSerializeProperty(string propertyName)
    {
        if (Include == GetTeacherRequestIncludes.All)
        {
            return true;
        }

        return propertyName switch
        {
            nameof(Induction) => Include.HasFlag(GetTeacherRequestIncludes.Induction),
            nameof(InitialTeacherTraining) => Include.HasFlag(GetTeacherRequestIncludes.InitialTeacherTraining),
            nameof(NpqQualifications) => Include.HasFlag(GetTeacherRequestIncludes.NpqQualifications),
            nameof(MandatoryQualifications) => Include.HasFlag(GetTeacherRequestIncludes.MandatoryQualifications),
            nameof(PendingNameChange) => Include.HasFlag(GetTeacherRequestIncludes.PendingDetailChanges),
            nameof(PendingDateOfBirthChange) => Include.HasFlag(GetTeacherRequestIncludes.PendingDetailChanges),
            _ => true
        };
    }
}

public record GetTeacherResponseQts
{
    public required DateOnly Awarded { get; init; }
    [SwaggerSchema(Nullable = false)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string CertificateUrl { get; init; }
}

public record GetTeacherResponseEyts
{
    public required DateOnly Awarded { get; init; }
    [SwaggerSchema(Nullable = false)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string CertificateUrl { get; init; }
}

public record GetTeacherResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required InductionStatus? Status { get; init; }
    [SwaggerSchema(Nullable = false)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string CertificateUrl { get; init; }
    [SwaggerSchema(Nullable = false)]
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
    [SwaggerSchema(Nullable = false)]
    public required string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTraining
{
    public required GetTeacherResponseInitialTeacherTrainingQualification Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingAgeRange AgeRange { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingProvider Provider { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required IEnumerable<GetTeacherResponseInitialTeacherTrainingSubject> Subjects { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingQualification
{
    [SwaggerSchema(Nullable = false)]
    public required string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingProvider
{
    [SwaggerSchema(Nullable = false)]
    public required string Name { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string Ukprn { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingSubject
{
    [SwaggerSchema(Nullable = false)]
    public required string Code { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string Name { get; init; }
}

public record GetTeacherResponseNpqQualificationsQualification
{
    [SwaggerSchema(Nullable = false)]
    public required DateOnly Awarded { get; init; }
    public required GetTeacherResponseNpqQualificationsQualificationType Type { get; init; }
    [SwaggerSchema(Nullable = false)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string CertificateUrl { get; init; }
}

public record GetTeacherResponseNpqQualificationsQualificationType
{
    public required NpqQualificationType Code { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string Name { get; init; }
}

public record GetTeacherResponseMandatoryQualificationsQualification
{
    public required DateOnly Awarded { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string Specialism { get; init; }
}
