using System;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public class CreateTeacherCommand
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string EmailAddress { get; set; }
        public CreateTeacherCommandAddress Address { get; set; }
        public Contact_GenderCode GenderCode { get; set; }
        public CreateTeacherCommandInitialTeacherTraining InitialTeacherTraining { get; set; }
        public CreateTeacherCommandQualification Qualification { get; set; }
        public string HusId { get; set; }
    }

    public class CreateTeacherCommandAddress
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class CreateTeacherCommandInitialTeacherTraining
    {
        public string ProviderUkprn { get; set; }
        public DateOnly ProgrammeStartDate { get; set; }
        public DateOnly ProgrammeEndDate { get; set; }
        public dfeta_ITTProgrammeType ProgrammeType { get; set; }
        public string Subject1 { get; set; }
        public string Subject2 { get; set; }
        public string Subject3 { get; set; }
        public dfeta_AgeRange? AgeRangeFrom { get; set; }
        public dfeta_AgeRange? AgeRangeTo { get; set; }
        public string IttQualificationValue { get; set; }
        public dfeta_ITTQualificationAim? IttQualificationAim { get; set; }
    }

    public class CreateTeacherCommandQualification
    {
        public string ProviderUkprn { get; set; }
        public string CountryCode { get; set; }
        public string Subject { get; set; }
        public string Subject2 { get; set; }
        public string Subject3 { get; set; }
        public dfeta_classdivision? Class { get; set; }
        public DateOnly? Date { get; set; }
        public string HeQualificationValue { get; set; }
    }
}
