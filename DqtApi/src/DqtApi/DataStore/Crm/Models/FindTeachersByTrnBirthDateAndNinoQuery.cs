using System;

namespace DqtApi.DataStore.Crm.Models
{
    public class FindTeachersByTrnBirthDateAndNinoQuery
    {
        public string Trn { get; set; }
        public DateTime? BirthDate { get; set; }
        public string NationalInsuranceNumber { get; set; }
    }
}
