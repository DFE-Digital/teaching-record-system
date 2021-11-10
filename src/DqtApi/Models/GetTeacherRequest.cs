using System;

namespace DqtApi.Models
{
    public struct GetTeacherRequest
    {
        public string TRN { get; set; }
        public DateTime BirthDate { get; set; }
        public string NationalInsuranceNumber { get; set; }
    }
}
