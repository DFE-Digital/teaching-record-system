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
    public required DateOnly? QtsDate { get; init; }
    public IEnumerable<GetTeacherResponseInitialTeacherTraining> InitialTeacherTraining { get; set; }
}

public record GetTeacherResponseInitialTeacherTraining
{
    public GetTeacherResponseInitialTeacherTrainingQualification Qualification { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public IttProgrammeType? ProgrammeType { get; set; }
    public IttOutcome? Result { get; set; }
    public GetTeacherResponseInitialTeacherTrainingAgeRange AgeRange { get; set; }
    public GetTeacherResponseInitialTeacherTrainingProvider Provider { get; set; }
    public IEnumerable<GetTeacherResponseInitialTeacherTrainingSubject> Subjects { get; set; }
}

public record GetTeacherResponseInitialTeacherTrainingQualification
{
    public string Name { get; set; }
}

public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public string Description { get; set; }
}

public record GetTeacherResponseInitialTeacherTrainingProvider
{
    public string Name { get; set; }
    public string Ukprn { get; set; }
}

public record GetTeacherResponseInitialTeacherTrainingSubject
{
    public string Code { get; init; }
    public string Name { get; init; }
}
