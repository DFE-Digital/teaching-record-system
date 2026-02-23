using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum DisconnectOneLoginReason
{
    [Display(Name = "Connected incorrectly")]
    ConnectedIncorrectly,
    [Display(Name = "New information shows it should not be connected")]
    NewInformation,
    [Display(Name = "Another reason")]
    AnotherReason
}
