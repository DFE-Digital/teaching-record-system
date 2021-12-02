using System;
using System.Text.Json.Serialization;
using DqtApi.DAL;
using Microsoft.Xrm.Sdk;

namespace DqtApi.Responses
{
    public class InitialTeacherTraining : LinkedEntity<dfeta_initialteachertraining>
    {
        public InitialTeacherTraining() { }

        [JsonIgnore]
        public Subject Subject1 => GetSubject(1);

        [JsonIgnore]
        public Subject Subject2 => GetSubject(2);

        [JsonIgnore]
        public Subject Subject3 => GetSubject(3);


        private Subject GetSubject(int subjectIndex)
        {
            var prefix = nameof(dfeta_ittsubject) + subjectIndex;
            var attributes = Entity.Attributes.MapCollection<object, AttributeCollection>(attribute => attribute.Value, prefix);

            return new Subject
            {
                Entity = new dfeta_ittsubject { Attributes = attributes }
            };
        }
        
        [JsonPropertyName("state")]
        public dfeta_initialteachertrainingState State => Entity.StateCode.Value;

        [JsonPropertyName("state_code")]
        public string StateName => FormattedValues[dfeta_qtsregistration.Fields.StateCode];

        [JsonPropertyName("programme_start_date")]
        public DateTime? ProgrammeStartDate => Entity.dfeta_ProgrammeStartDate;

        [JsonPropertyName("programme_end_date")]
        public DateTime? ProgrammeEndDate => Entity.dfeta_ProgrammeEndDate;

        [JsonPropertyName("programme_type")]
        public string ProgrammeType => FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_ProgrammeType);

        [JsonPropertyName("result")]
        public string Result => FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Result);

        [JsonPropertyName("subject1")]
        public string Subject1Id => FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Subject1Id);

        [JsonPropertyName("subject2")]
        public string Subject2Id => FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Subject2Id);

        [JsonPropertyName("subject3")]
        public string Subject3Id => FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Subject3Id);

        [JsonPropertyName("qualification")]
        public string Qualification => FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId);

        [JsonPropertyName("subject1_code")]
        public string Subject1Code => Subject1?.Code;

        [JsonPropertyName("subject2_code")]
        public string Subject2Code => Subject2?.Code;

        [JsonPropertyName("subject3_code")]
        public string Subject3Code => Subject3?.Code;
    }
}
