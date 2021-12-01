using System;
using System.Text.Json.Serialization;

namespace DqtApi.Responses
{
    public class QualifiedTeacherStatus : LinkedEntity<dfeta_qtsregistration>
    {
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name => Entity.dfeta_name;

        [JsonPropertyName("state")]
        public dfeta_qtsregistrationState State => Entity.StateCode.Value;

        [JsonPropertyName("state_name")]
        public string StateName => FormattedValues[dfeta_qtsregistration.Fields.StateCode];

        [JsonPropertyName("qts_date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? QTSDate => Entity.dfeta_QTSDate;                

        public QualifiedTeacherStatus() { }        
    }
}
