#nullable disable
using System;

namespace QualifiedTeachersApi.DataStore.Crm.Models;

public class FindTeachersByTrnBirthDateAndNinoQuery
{
    public string Trn { get; set; }
    public DateTime? BirthDate { get; set; }
    public string NationalInsuranceNumber { get; set; }
}
