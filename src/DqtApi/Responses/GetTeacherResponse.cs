using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DqtApi.Models;

namespace DqtApi.Responses
{
    public class GetTeacherResponse
    {
        private readonly Contact _teacher;

        [JsonPropertyName("trn")]
        public string Trn => _teacher.dfeta_TRN;

        [JsonPropertyName("ni_number")]
        public string NationalInsuranceNumber => _teacher.dfeta_NINumber;

        [JsonPropertyName("qualified_teacher_status")]
        public QualifiedTeacherStatus QualifiedTeacherStatus { get; set; }

        [JsonPropertyName("induction")]
        public Induction Induction { get; set; }

        [JsonPropertyName("initial_teacher_training")]
        public InitialTeacherTraining InitialTeacherTraining { get; set; }

        [JsonPropertyName("qualifications")]
        public IEnumerable<Qualification> Qualifications { get; set; }

        [JsonPropertyName("name")]
        public string Name => _teacher.FullName;

        [JsonPropertyName("dob")]
        public DateTime? DateOfBirth => _teacher.BirthDate;

        [JsonPropertyName("active_alert")]
        public bool? ActiveAlert => _teacher.dfeta_ActiveSanctions;

        [JsonPropertyName("state")]
        public ContactState State => _teacher.StateCode.Value;

        [JsonPropertyName("state_name")]
        public string StateName => _teacher.FormattedValues[Contact.Fields.StateCode];

        public GetTeacherResponse(Contact teacher)
        {            
            _teacher = teacher;

            QualifiedTeacherStatus = _teacher.Extract<dfeta_qtsregistration, QualifiedTeacherStatus>();
            Induction = _teacher.Extract<dfeta_induction, Induction>();
            // todo check we should return first, or should we return unique active record? see teacherpolicy.xml
            InitialTeacherTraining = _teacher.Extract<dfeta_initialteachertraining, InitialTeacherTraining>();

            Qualifications = _teacher.dfeta_contact_dfeta_qualification?.Select(qualification => new Qualification(qualification))
                ?? new List<Qualification>();
        }
    }
}
