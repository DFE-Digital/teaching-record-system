using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;

public enum KeepingRecordSeparateReason
{
    [Display(Name = "The records do not match")]
    RecordDoesNotMatch = 0,
    [Display(Name = "Another reason")]
    AnotherReason = 1
}
