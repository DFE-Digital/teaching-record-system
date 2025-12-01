using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum PersonNameChangeReason
{
    [Display(Name = "Name has changed due to marriage or civil partnership")]
    MarriageOrCivilPartnership,
    [Display(Name = "Name has changed by deed poll or another legal process")]
    DeedPollOrOtherLegalProcess,
    [Display(Name = "Correcting an error")]
    CorrectingAnError
}
