using System;
using System.Text.Json.Serialization;
using DqtApi.Models;

namespace DqtApi.V1.Responses
{
    public class Induction : LinkedEntity<dfeta_induction>
    {
        public Induction() { }

        [JsonPropertyName("start_date")]
        public DateTime? StartDate => Entity.dfeta_StartDate;

        [JsonPropertyName("completion_date")]
        public DateTime? CompletionDate => Entity.dfeta_CompletionDate;

        [JsonPropertyName("status")]
        public string InductionStatusName => FormattedValues[dfeta_induction.Fields.dfeta_InductionStatus];

        [JsonPropertyName("state")]
        public dfeta_inductionState State => Entity.StateCode.Value;

        [JsonPropertyName("state_name")]
        public string StateName => FormattedValues[dfeta_induction.Fields.StateCode];
    }
}
