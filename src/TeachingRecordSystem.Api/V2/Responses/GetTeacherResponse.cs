#nullable disable
namespace TeachingRecordSystem.Api.V2.Responses;

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
    public IEnumerable<object> InitialTeacherTraining { get; set; }
    public bool AllowPIIUpdates { get; set; }
}

public class GetTeacherResponseEarlyYearsStatus
{
    public string Value { get; set; }
    public string Name { get; set; }
}
