#nullable disable
using System;
using System.Collections.Generic;
using QualifiedTeachersApi.V2.ApiModels;

namespace QualifiedTeachersApi.V2.Responses;

public class GetTeacherResponse
{
    public string Trn { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string NationalInsuranceNumber { get; set; }
    public bool HasActiveSanctions { get; set; }
    public DateOnly? QtsDate { get; set; }
    public DateOnly? EytsDate { get; set; }
    public string HusId { get; set; }
    public GetTeacherResponseEarlyYearsStatus EarlyYearsStatus { get; set; }
    public IEnumerable<GetTeacherResponseInitialTeacherTraining> InitialTeacherTraining { get; set; }
}

public class GetTeacherResponseEarlyYearsStatus
{
    public string Value { get; set; }
    public string Name { get; set; }
}

public class GetTeacherResponseInitialTeacherTraining
{
    public DateOnly? ProgrammeStartDate { get; set; }
    public DateOnly? ProgrammeEndDate { get; set; }
    public IttProgrammeType? ProgrammeType { get; set; }
    public IttOutcome? Result { get; set; }
    public GetTeacherResponseInitialTeacherTrainingProvider Provider { get; set; }
    public string HusId { get; set; }
    public bool Active { get; set; }
}

public class GetTeacherResponseInitialTeacherTrainingProvider
{
    public string Ukprn { get; set; }
}
