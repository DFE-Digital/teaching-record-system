using System;

namespace DqtApi.DataStore.Crm
{
    public class FindTeachersByTrnBirthDateAndNinoQuery
    {
        public string Trn { get; set; }
        public DateTime? BirthDate { get; set; }
        public string NationalInsuranceNumber { get; set; }
    }
}
