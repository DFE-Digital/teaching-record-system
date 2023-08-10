#nullable disable
using Optional;

namespace TeachingRecordSystem.Core.Dqt.Models;

public class UpdateTeacherCommand
{
    public Guid TeacherId { get; set; }
    public UpdateTeacherCommandInitialTeacherTraining InitialTeacherTraining { get; set; }
    public UpdateTeacherCommandQualification Qualification { get; set; }
    public string Trn { get; set; }
    public Option<string> HusId { get; set; }
    public Option<string> SlugId { get; set; }
    public Option<string> StatedFirstName { get; set; }
    public Option<string> StatedMiddleName { get; set; }
    public Option<string> StatedLastName { get; set; }
    public Option<string> FirstName { get; set; }
    public Option<string> MiddleName { get; set; }
    public Option<string> LastName { get; set; }
    public Option<string> EmailAddress { get; set; }
    public Option<Contact_GenderCode> GenderCode { get; set; }
    public Option<DateTime> DateOfBirth { get; set; }
}

public class UpdateTeacherCommandInitialTeacherTraining
{
    public string ProviderUkprn { get; set; }
    public DateOnly? ProgrammeStartDate { get; set; }
    public DateOnly? ProgrammeEndDate { get; set; }
    public dfeta_ITTProgrammeType ProgrammeType { get; set; }
    public string Subject1 { get; set; }
    public string Subject2 { get; set; }
    public string Subject3 { get; set; }
    public dfeta_AgeRange? AgeRangeFrom { get; set; }
    public dfeta_AgeRange? AgeRangeTo { get; set; }
    public string IttQualificationValue { get; set; }
    public dfeta_ITTQualificationAim? IttQualificationAim { get; set; }
    public string TrainingCountryCode { get; set; }
    public dfeta_ITTResult? Result { get; set; }
}

public class UpdateTeacherCommandQualification
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
