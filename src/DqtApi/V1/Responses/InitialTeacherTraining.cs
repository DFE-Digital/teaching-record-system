using System;
using System.Text.Json.Serialization;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.V1.Responses
{
    public class InitialTeacherTraining
    {
        [JsonPropertyName("state")]
        public dfeta_initialteachertrainingState State { get; set; }

        [JsonPropertyName("state_code")]
        public string StateName { get; set; }

        [JsonPropertyName("programme_start_date")]
        public DateTime? ProgrammeStartDate { get; set; }

        [JsonPropertyName("programme_end_date")]
        public DateTime? ProgrammeEndDate { get; set; }

        [JsonPropertyName("programme_type")]
        public string ProgrammeType { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("subject1")]
        public string Subject1Id { get; set; }

        [JsonPropertyName("subject2")]
        public string Subject2Id { get; set; }

        [JsonPropertyName("subject3")]
        public string Subject3Id { get; set; }

        [JsonPropertyName("qualification")]
        public string Qualification { get; set; }

        [JsonPropertyName("subject1_code")]
        public string Subject1Code { get; set; }

        [JsonPropertyName("subject2_code")]
        public string Subject2Code { get; set; }

        [JsonPropertyName("subject3_code")]
        public string Subject3Code { get; set; }
    }
}
