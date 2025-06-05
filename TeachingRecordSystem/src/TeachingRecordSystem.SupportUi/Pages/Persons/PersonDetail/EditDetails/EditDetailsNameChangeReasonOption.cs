using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public enum EditDetailsNameChangeReasonOption
{
    [Display(Name = "Name has changed due to marriage or civil partnership")]
    MarriageOrCivilPartnership,
    [Display(Name = "Name has changed by deed poll or another legal process")]
    DeedPollOrOtherLegalProcess,
    [Display(Name = "Correcting an error")]
    CorrectingAnError
}
