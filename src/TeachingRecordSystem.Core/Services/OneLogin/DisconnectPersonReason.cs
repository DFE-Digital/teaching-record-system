using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public enum DisconnectPersonReason
{
    [Display(Name = "Connected incorrectly")]
    ConnectedIncorrectly,
    [Display(Name = "New information shows it should not be connected")]
    NewInformation,
    [Display(Name = "Another reason")]
    AnotherReason
}
