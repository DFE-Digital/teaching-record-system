using System.Text.Json.Serialization;
using DqtApi.Models;

namespace DqtApi.Responses
{
    public class GetTeacherResponse
    {
        [JsonPropertyName("trn")]
        public string Trn { get; set; }
        [JsonPropertyName("ni_number")]
        public string NationalInsuranceNumber { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("dob")]
        public string DateOfBirth { get; set; }
        [JsonPropertyName("active_alert")]
        public bool ActiveAlert { get; set; }
        [JsonPropertyName("state")]
        public int State { get; set; }
        [JsonPropertyName("state_name")]
        public string StateName { get; set; }

        public GetTeacherResponse(Teacher teacher)
        {
            Trn = teacher.Trn;
            Name = teacher.Name;
            NationalInsuranceNumber = teacher.NationalInsuranceNumber;
            // TODO other fields
        }
    }
}