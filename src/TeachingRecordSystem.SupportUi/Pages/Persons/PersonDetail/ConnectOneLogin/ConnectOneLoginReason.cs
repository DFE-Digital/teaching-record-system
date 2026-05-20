using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public enum ConnectOneLoginReason
{
    [Display(Name = "The system could not find a match automatically")]
    SystemCouldNotMatch,
    [Display(Name = "This record was connected to the wrong GOV.UK One Login")]
    ConnectedToWrongOneLogin,
    [Display(Name = "Another reason")]
    AnotherReason
}
