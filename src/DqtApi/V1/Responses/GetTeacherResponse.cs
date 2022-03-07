using System;
using System.Text.Json.Serialization;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.V1.Responses
{
    public class GetTeacherResponse
    {
        [JsonPropertyName("trn")]
        public string Trn { get; set; }

        [JsonPropertyName("ni_number")]
        public string NationalInsuranceNumber { get; set; }

        [JsonPropertyName("qualified_teacher_status")]
        public QualifiedTeacherStatus QualifiedTeacherStatus { get; set; }

        [JsonPropertyName("induction")]
        public Induction Induction { get; set; }

        [JsonPropertyName("initial_teacher_training")]
        public InitialTeacherTraining InitialTeacherTraining { get; set; }

        [JsonPropertyName("qualifications")]
        public Qualification[] Qualifications { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("dob")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("active_alert")]
        public bool? ActiveAlert { get; set; }

        [JsonPropertyName("state")]
        public ContactState State { get; set; }

        [JsonPropertyName("state_name")]
        public string StateName { get; set; }
    }
}
