using System;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public class UpdateTeacherCommand
    {
        public string TeacherId { get; set; }
        public UpdateTeacherCommandInitialTeacherTraining InitialTeacherTraining { get; set; }
        public UpdateTeacherCommandQualification Qualification { get; set; }
    }

    public class UpdateTeacherCommandInitialTeacherTraining
    {
        public string ProviderUkprn { get; set; }
        public DateOnly? ProgrammeStartDate { get; set; }
        public DateOnly? ProgrammeEndDate { get; set; }
        public dfeta_ITTProgrammeType ProgrammeType { get; set; }
        public string Subject1 { get; set; }
        public string Subject2 { get; set; }
        public dfeta_AgeRange? AgeRangeFrom { get; set; }
        public dfeta_AgeRange? AgeRangeTo { get; set; }
    }

    public class UpdateTeacherCommandQualification
    {
        public string ProviderUkprn { get; set; }
        public string CountryCode { get; set; }
        public string Subject { get; set; }
        public dfeta_classdivision? Class { get; set; }
        public DateOnly? Date { get; set; }
    }
}
