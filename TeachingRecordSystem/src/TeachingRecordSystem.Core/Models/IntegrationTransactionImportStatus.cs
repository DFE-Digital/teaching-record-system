using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum IntegrationTransactionImportStatus
{
    [Description("Success")]
    [Display(Name = "Success")]
    Success = 0,

    [Description("Failed")]
    [Display(Name = "Failed")]
    Failed = 1,

    [Description("In progress")]
    [Display(Name = "In progress")]
    InProgress = 2
}
