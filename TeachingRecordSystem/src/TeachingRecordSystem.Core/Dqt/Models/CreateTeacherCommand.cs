#nullable disable
namespace TeachingRecordSystem.Core.Dqt.Models;

public class CreateTeacherCommand
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string StatedFirstName { get; set; }
    public string StatedMiddleName { get; set; }
    public string StatedLastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string EmailAddress { get; set; }
    public CreateTeacherCommandAddress Address { get; set; }
    public Contact_GenderCode GenderCode { get; set; }
    public CreateTeacherCommandInitialTeacherTraining InitialTeacherTraining { get; set; }
    public CreateTeacherCommandQualification Qualification { get; set; }
    public string HusId { get; set; }
    public CreateTeacherType TeacherType { get; set; }
    public CreateTeacherRecognitionRoute? RecognitionRoute { get; set; }
    public DateOnly? QtsDate { get; set; }
    public bool? InductionRequired { get; set; }
    public bool? UnderNewOverseasRegulations { get; set; }
    public string SlugId { get; set; }
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
    public dfeta_ITTProgrammeType? ProgrammeType { get; set; }
    public string Subject1 { get; set; }
    public string Subject2 { get; set; }
    public string Subject3 { get; set; }
    public dfeta_AgeRange? AgeRangeFrom { get; set; }
    public dfeta_AgeRange? AgeRangeTo { get; set; }
    public string IttQualificationValue { get; set; }
    public dfeta_ITTQualificationAim? IttQualificationAim { get; set; }
    public string TrainingCountryCode { get; set; }
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

public enum CreateTeacherType
{
    TraineeTeacher = 0,
    OverseasQualifiedTeacher = 1
}

public enum CreateTeacherRecognitionRoute
{
    Scotland = 1,
    NorthernIreland = 2,
    EuropeanEconomicArea = 3,
    OverseasTrainedTeachers = 4
}
