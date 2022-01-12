using System;
using System.Text.Json.Serialization;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.V1.Responses
{
    public class QualifiedTeacherStatus : LinkedEntity<dfeta_qtsregistration>
    {
        [JsonPropertyName("name")]
        public string Name => Entity.dfeta_name;

        [JsonPropertyName("state")]
        public dfeta_qtsregistrationState State => Entity.StateCode.Value;

        [JsonPropertyName("state_name")]
        public string StateName => FormattedValues[dfeta_qtsregistration.Fields.StateCode];

        [JsonPropertyName("qts_date")]
        public DateTime? QTSDate => Entity.dfeta_QTSDate;                

        public QualifiedTeacherStatus() { }        
    }
}
