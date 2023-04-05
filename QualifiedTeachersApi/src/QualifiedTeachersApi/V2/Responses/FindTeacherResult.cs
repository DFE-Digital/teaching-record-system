#nullable disable
using System;
using System.Collections.Generic;

namespace QualifiedTeachersApi.V2.Responses;

public class FindTeacherResult
{
    public string Trn { get; set; }
    public List<string> EmailAddresses { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string NationalInsuranceNumber { get; set; }
    public string Uid { get; set; }
    public bool HasActiveSanctions { get; set; }
}
