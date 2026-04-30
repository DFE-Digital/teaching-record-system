using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

public enum ConnectPersonReason
{
    [Display(Name = "Data loss or incomplete information")]
    DataLossOrIncompleteInformation,
    [Display(Name = "New information received")]
    NewInformationReceived,
    [Display(Name = "Another reason")]
    AnotherReason
}
