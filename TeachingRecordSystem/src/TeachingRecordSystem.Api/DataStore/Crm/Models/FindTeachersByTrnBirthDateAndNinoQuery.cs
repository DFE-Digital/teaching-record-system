#nullable disable

namespace TeachingRecordSystem.Api.DataStore.Crm.Models;

public class FindTeachersByTrnBirthDateAndNinoQuery
{
    public string Trn { get; set; }
    public DateTime? BirthDate { get; set; }
    public string NationalInsuranceNumber { get; set; }
}
