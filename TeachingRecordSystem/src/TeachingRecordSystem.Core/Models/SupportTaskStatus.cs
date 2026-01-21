using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum SupportTaskStatus
{
    [Display(Name = "Not started")]
    Open = 0,

    [Display(Name = "Completed")]
    Closed = 1,

    [Display(Name = "In progress")]
    InProgress = 2
}
